using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmScripMasterCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmScripMasterCommandHandler : IRequestHandler<ImportCmScripMasterCommand, ImportResultDto>
{
    private readonly IRepository<CmScripMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportCmScripMasterCommandHandler(
        IRepository<CmScripMaster> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportCmScripMasterCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Pre-load existing ISINs for duplicate check
        var existingRecords = await _repo.FindAsync(
            s => s.TenantId    == tenantId &&
                 s.Exchange    == request.Exchange &&
                 s.TradingDate == request.TradingDate,
            cancellationToken);
        var existingIsins = existingRecords
            .Select(s => s.ISIN)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var errors  = new List<ImportErrorDto>();
        var chunk   = new List<CmScripMaster>();
        int rowNum = 0, created = 0, skipped = 0;

        // BSE uses pipe-delimited; NSE uses CSV
        bool isBse = string.Equals(request.Exchange, "BSE", StringComparison.OrdinalIgnoreCase);

        using var reader = new StreamReader(request.FileStream);

        // Read header and build column-name → index map
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine == null)
            return new ImportResultDto(0, 0, errors);

        var headers = isBse
            ? headerLine.Split('|')
            : CsvParser.ParseLine(headerLine);

        var colMap = headers
            .Select((h, i) => (Name: h.Trim(), Index: i))
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            rowNum++;
            try
            {
                var fields = isBse
                    ? line.Split('|')
                    : CsvParser.ParseLine(line);

                string Get(string col) =>
                    colMap.TryGetValue(col, out var idx) && idx < fields.Length
                        ? fields[idx].Trim()
                        : string.Empty;

                string symbol, series, isin, name, instrumentType;
                decimal faceValue, tickSize;
                int lotSize;

                if (isBse)
                {
                    // BSE scrip master (pipe-delimited):
                    // Scrip_Code|Exchange_code|Scrip_Id|Scrip_Name|...|Face_Value|Market_Lot|
                    // Tick_Size|BSE_Exclusive|Status|...|ISIN_CODE|...|Security_Type_Flag|...
                    symbol         = Get("Scrip_Code");
                    series         = Get("Scrip_Id");      // NSE equivalent symbol / series
                    isin           = Get("ISIN_CODE");
                    name           = Get("Scrip_Name");
                    instrumentType = string.IsNullOrEmpty(Get("Security_Type_Flag"))
                                       ? Get("Instrument_Type")
                                       : Get("Security_Type_Flag");

                    _ = decimal.TryParse(Get("Face_Value"), out var fvRaw);
                    faceValue = fvRaw / 100m;  // BSE stores in paise

                    _ = int.TryParse(Get("Market_Lot"), out lotSize);
                    _ = decimal.TryParse(Get("Tick_Size"), out tickSize);
                }
                else
                {
                    // NSE security master (CSV):
                    // FinInstrmId,TckrSymb,SctySrs,FinInstrmNm,ISIN,NewBrdLotQty,ParVal,
                    // SctyTpFlg,...
                    symbol = Get("TckrSymb");
                    series = Get("SctySrs");
                    isin   = Get("ISIN");
                    name   = Get("FinInstrmNm");

                    _ = decimal.TryParse(Get("ParVal"), out var parVal);
                    faceValue = parVal / 100m;  // NSE stores face value in paise

                    _ = int.TryParse(Get("NewBrdLotQty"), out lotSize);
                    _ = decimal.TryParse(Get("TickSz"), out tickSize);

                    // Decode SctyTpFlg → instrument type (matches reference Scheduler.cs logic)
                    instrumentType = Get("SctyTpFlg") switch
                    {
                        "0" => "EQ",
                        "1" => "PREF",
                        "2" => "DB",
                        "3" => "WT",
                        _   => Get("SttlmTp")
                    };
                }

                if (string.IsNullOrEmpty(isin) || string.IsNullOrEmpty(symbol))
                {
                    skipped++;
                    continue;
                }

                if (existingIsins.Contains(isin))
                {
                    skipped++;
                    continue;
                }

                existingIsins.Add(isin);
                chunk.Add(new CmScripMaster
                {
                    CmScripMasterId = Guid.NewGuid(),
                    TenantId        = tenantId,
                    Exchange        = request.Exchange,
                    TradingDate     = request.TradingDate,
                    Symbol          = symbol,
                    ISIN            = isin,
                    Series          = series,
                    Name            = name,
                    FaceValue       = faceValue,
                    LotSize         = lotSize,
                    TickSize        = tickSize,
                    InstrumentType  = instrumentType
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
}
