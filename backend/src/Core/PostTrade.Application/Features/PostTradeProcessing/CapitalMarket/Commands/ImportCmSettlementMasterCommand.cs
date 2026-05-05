using System.Globalization;
using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmSettlementMasterCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmSettlementMasterCommandHandler : IRequestHandler<ImportCmSettlementMasterCommand, ImportResultDto>
{
    private readonly IRepository<CmSettlementMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    // Date formats found in NSDL / NSE / BSE settlement files
    private static readonly string[] DateFormats =
        ["dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy"];

    public ImportCmSettlementMasterCommandHandler(
        IRepository<CmSettlementMaster> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportCmSettlementMasterCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var errors  = new List<ImportErrorDto>();
        var chunk   = new List<CmSettlementMaster>();
        int rowNum = 0, created = 0, skipped = 0;

        // Pre-load existing settlement numbers for this exchange+date to avoid DB round-trips
        var existingRecords = await _repo.FindAsync(
            s => s.TenantId == tenantId &&
                 s.Exchange  == request.Exchange &&
                 s.TradingDate == request.TradingDate,
            cancellationToken);
        var existingSettlementNos = existingRecords
            .Select(s => s.SettlementNo)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(request.FileStream);

        // Read header row and build column-name → index map
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine == null)
            return new ImportResultDto(0, 0, errors);

        var headers = CsvParser.ParseLine(headerLine);
        var colMap  = headers
            .Select((h, i) => (Name: h.Trim(), Index: i))
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

        // NSDL format has MktTpAndId; NSE/BSE settlement CSV has SttlmTp
        bool isNsdlFormat = colMap.ContainsKey("MktTpAndId");

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            rowNum++;
            try
            {
                var fields = CsvParser.ParseLine(line);

                string Get(string col) =>
                    colMap.TryGetValue(col, out var idx) && idx < fields.Length
                        ? fields[idx].Trim()
                        : string.Empty;

                string settlementNo, settlementType, periodFromRaw, payInRaw, payOutRaw;

                if (isNsdlFormat)
                {
                    // NSDL settlement file:
                    // Src,CntrlSctiesDpstryPtcpt,LineNb,Xchg,ClrSysId,MktTpAndId,
                    // SctiesSttlmTxId,SttlmPrdFr,SttlmPrdTo,NSLDDdlnDt,PayInDt,PyoutDt,...
                    settlementNo   = Get("SctiesSttlmTxId");
                    settlementType = Get("MktTpAndId");
                    periodFromRaw  = Get("SttlmPrdFr");
                    payInRaw       = Get("PayInDt");
                    payOutRaw      = Get("PyoutDt");
                }
                else
                {
                    // NSE / BSE settlement file:
                    // Sgmt,Src,...,SttlmTp,SctiesSttlmTxId,...,
                    // OrgnlTradDtTm,...,FndsPayInDtAndTm,FndsPayOutDtAndTm,...
                    settlementNo   = Get("SctiesSttlmTxId");
                    settlementType = Get("SttlmTp");
                    periodFromRaw  = Get("OrgnlTradDtTm");
                    payInRaw       = Get("FndsPayInDtAndTm");
                    payOutRaw      = Get("FndsPayOutDtAndTm");
                }

                if (string.IsNullOrEmpty(settlementNo))
                {
                    skipped++;
                    continue;
                }

                var tradingDate = TryParseDate(periodFromRaw) ?? request.TradingDate;
                var payInDate   = TryParseDate(payInRaw)      ?? tradingDate;
                var payOutDate  = TryParseDate(payOutRaw)     ?? tradingDate;

                if (existingSettlementNos.Contains(settlementNo))
                {
                    skipped++;
                    continue;
                }

                existingSettlementNos.Add(settlementNo);
                chunk.Add(new CmSettlementMaster
                {
                    CmSettlementMasterId = Guid.NewGuid(),
                    TenantId             = tenantId,
                    Exchange             = request.Exchange,
                    TradingDate          = tradingDate,
                    SettlementNo         = settlementNo,
                    SettlementType       = settlementType,
                    PayInDate            = payInDate,
                    PayOutDate           = payOutDate
                });
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _repo.AddRangeAsync(chunk, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _unitOfWork.ClearTracking();
                    chunk.Clear();
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNum, ex.Message));
                skipped++;
            }
        }

        if (chunk.Count > 0)
        {
            await _repo.AddRangeAsync(chunk, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _unitOfWork.ClearTracking();
        }

        return new ImportResultDto(created, skipped, errors);
    }

    /// <summary>
    /// Parses a date/datetime string from NSDL/NSE/BSE files.
    /// Handles DD-MM-YYYY, YYYY-MM-DD, DD/MM/YYYY and ISO datetime (YYYY-MM-DDTHH:MM:SS).
    /// </summary>
    private static DateOnly? TryParseDate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        // Strip time component if present (e.g. "2025-01-15T09:15:00")
        var datePart = raw.Contains('T') ? raw[..raw.IndexOf('T')] : raw;

        if (DateTime.TryParseExact(datePart, DateFormats,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return DateOnly.FromDateTime(dt);

        if (DateTime.TryParse(datePart, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt2))
            return DateOnly.FromDateTime(dt2);

        return null;
    }
}
