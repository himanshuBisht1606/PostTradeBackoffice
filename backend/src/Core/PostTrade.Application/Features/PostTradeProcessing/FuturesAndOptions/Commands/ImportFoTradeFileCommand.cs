using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record ImportFoTradeFileCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,        // NFO | BFO
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportFoTradeFileCommandHandler : IRequestHandler<ImportFoTradeFileCommand, ImportResultDto>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IRepository<FoTrade> _tradeRepo;
    private readonly IRepository<FoTradeBook> _tradeBookRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IRepository<FoContractMaster> _contractMasterRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoTradeFileCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoTrade> tradeRepo,
        IRepository<FoTradeBook> tradeBookRepo,
        IRepository<FoFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IRepository<FoContractMaster> contractMasterRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _tradeRepo = tradeRepo;
        _tradeBookRepo = tradeBookRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _contractMasterRepo = contractMasterRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoTradeFileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Prerequisite: contract master must exist for this exchange and trading date
        var hasContract = await _contractMasterRepo.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
            cancellationToken);
        if (hasContract == null)
            throw new PrerequisiteNotMetException(
                $"FO Contract Master missing for Exchange '{request.Exchange}' on {request.TradingDate:yyyy-MM-dd}. Import the contract master file first.");

        // Re-import: if a completed batch already exists, delete old trades and reuse the batch
        var existingBatch = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.Trade &&
            b.Exchange == request.Exchange &&
            b.TradingDate == request.TradingDate &&
            b.Status == FoImportStatus.Completed,
            cancellationToken);

        if (existingBatch != null)
        {
            await _unitOfWork.ExecuteDeleteAsync<FoTrade>(
                t => t.TenantId == tenantId && t.BatchId == existingBatch.BatchId,
                cancellationToken);
            await _unitOfWork.ExecuteDeleteAsync<FoTradeBook>(
                t => t.TenantId == tenantId && t.BatchId == existingBatch.BatchId,
                cancellationToken);

            existingBatch.Status = FoImportStatus.Processing;
            existingBatch.FileName = request.FileName;
            existingBatch.StartedAt = DateTime.UtcNow;
            existingBatch.CompletedAt = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var batch = existingBatch ?? new FoFileImportBatch
        {
            BatchId = Guid.NewGuid(),
            TenantId = tenantId,
            FileType = FoFileType.Trade,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = FoImportStatus.Processing,
            TriggerSource = Enum.TryParse<FoTriggerSource>(request.TriggerSource, out var src) ? src : FoTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };

        if (existingBatch == null)
        {
            await _batchRepo.AddAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Build client lookup: clientCode → (ClientId, ClientName, StateCode)
        var clients = await _clientRepo.GetAllAsync(cancellationToken);
        var clientMap = clients
            .Where(c => !string.IsNullOrEmpty(c.ClientCode))
            .ToDictionary(
                c => c.ClientCode!,
                c => (c.ClientId, c.ClientName, c.StateCode),
                StringComparer.OrdinalIgnoreCase);

        // Build contract master lookup by FinInstrmId for enrichment (lot size, underlying)
        var contracts = await _contractMasterRepo.FindAsync(
            c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
            cancellationToken);
        var contractMap = contracts.ToDictionary(
            c => c.FinInstrmId,
            c => c,
            StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        var chunk = new List<FoTrade>();
        var chunkBook = new List<FoTradeBook>();
        int rowNum = 0, created = 0, skipped = 0;

        // File columns (NSE/BSE FO trade file — 46 columns):
        // 0=TradDt,1=BizDt,2=Sgmt,3=Src,4=Xchg,5=ClrMmbId,6=Brkr,7=FinInstrmTp,8=FinInstrmId,
        // 9=ISIN,10=TckrSymb,11=SctySrs,12=XpryDt,13=FininstrmActlXpryDt,14=StrkPric,15=OptnTp,
        // 16=FinInstrmNm,17=ClntTp,18=ClntId,19=FullyExctdConfSnt,20=OrgnlCtdnPtcptId,21=CtdnPtcptId,
        // 22=SttlmTp,23=SctiesSttlmTxId,24=BuySellInd,25=TradQty,26=NewBrdLotQty,27=Pric,28=UnqTradIdr,
        // 29=RptdTxSts,30=TradDtTm,31=UpdDt,32=OrdrRef,33=OrdrDtTm,34=InstgUsr,35=CtclId,
        // 36=TradRegnOrgn,37=OrdrTp,38=BlckDealInd,39=SttlmCycl,40=MktTpandId,41=Rmks,42-45=Rsvd
        using var reader = new StreamReader(request.FileStream);
        string? line;
        bool isHeader = true;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (isHeader) { isHeader = false; continue; }

            rowNum++;
            try
            {
                var f = CsvParser.ParseLine(line);
                if (f.Length < 29)  // minimum required columns
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var clntId = f[18].Trim();
                string? clientName = null;
                string? clientStateCode = null;
                Guid? clientId = null;

                if (!string.IsNullOrEmpty(clntId) && clientMap.TryGetValue(clntId, out var clientInfo))
                {
                    clientId = clientInfo.ClientId;
                    clientName = clientInfo.ClientName;
                    clientStateCode = clientInfo.StateCode;
                }
                else if (!string.IsNullOrEmpty(clntId))
                {
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Client '{clntId}' not found in master" });
                }

                // Contract master enrichment
                var finInstrmId = f[8].Trim();
                long lotSize = 0;
                string underlyingSymbol = string.Empty;
                contractMap.TryGetValue(finInstrmId, out var contract);
                if (contract != null)
                {
                    lotSize = contract.NewBrdLotQty > 0 ? contract.NewBrdLotQty : contract.MinLot;
                    underlyingSymbol = contract.UndrlygFinInstrmId;
                }

                var tradQty = long.TryParse(f[25].Trim(), out var qty) ? qty : 0;
                var pric = decimal.TryParse(f[27].Trim(), out var p) ? p : 0m;
                var finInstrmTp = f[7].Trim();
                var optnTp = f[15].Trim();
                var contractType = MapInstrumentType(finInstrmTp);
                var optionType = ResolveOptionType(optnTp);
                var xpryDt = f[12].Trim();
                var expiryDate = ParseExpiryDate(xpryDt);

                // ── Staging row (raw exchange field names) ────────────────────
                var trade = new FoTrade
                {
                    TradeRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    UniqueTradeId = f[28].Trim(),
                    TradDt = request.TradingDate,
                    Sgmt = f[2].Trim(),
                    Src = f[3].Trim(),
                    Exchange = request.Exchange,
                    TradngMmbId = f[6].Trim(),
                    FinInstrmTp = finInstrmTp,
                    FinInstrmId = finInstrmId,
                    Isin = f[9].Trim(),
                    TckrSymb = f[10].Trim(),
                    XpryDt = xpryDt,
                    ExpiryDate = expiryDate,
                    StrkPric = decimal.TryParse(f[14].Trim(), out var sp) ? sp : 0,
                    OptnTp = optnTp,
                    FinInstrmNm = f[16].Trim(),
                    InstrumentType = contractType,
                    UnderlyingSymbol = underlyingSymbol,
                    LotSize = lotSize,
                    ClntTp = f[17].Trim(),
                    ClntId = clntId,
                    CtclId = f.Length > 35 ? NullIfBlank(f[35]) : null,      // CtclId is at index 35
                    OrgnlCtdnPtcptId = f.Length > 20 ? NullIfBlank(f[20]) : null,
                    ClientId = clientId,
                    ClientName = clientName,
                    ClientStateCode = clientStateCode,
                    SttlmTp = f[22].Trim(),
                    SctiesSttlmTxId = f[23].Trim(),
                    BuySellInd = f[24].Trim(),
                    TradQty = tradQty,
                    NewBrdLotQty = long.TryParse(f[26].Trim(), out var lot) ? lot : 0,
                    Pric = pric,
                    TradeValue = tradQty * pric,
                    NumLots = lotSize > 0 ? Math.Round((decimal)tradQty / lotSize, 4) : 0,
                    // Extended fields
                    TradDtTm   = f.Length > 30 ? NullIfBlank(f[30]) : null,
                    RptdTxSts  = f.Length > 29 ? NullIfBlank(f[29]) : null,
                    OrdrRef    = f.Length > 32 ? NullIfBlank(f[32]) : null,
                    SttlmCycl  = f.Length > 39 ? NullIfBlank(f[39]) : null,
                    MktTpandId = f.Length > 40 ? NullIfBlank(f[40]) : null,
                };

                // Parse trade datetime for the trade book.
                // Exchange timestamps (e.g. "2026-02-25T09:15:11") carry no timezone suffix.
                // Npgsql requires DateTimeKind.Utc for timestamptz columns, so we specify Utc.
                DateTime? tradeDtTm = null;
                if (!string.IsNullOrWhiteSpace(trade.TradDtTm) &&
                    DateTime.TryParse(trade.TradDtTm, null,
                        System.Globalization.DateTimeStyles.AssumeUniversal |
                        System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsedDt))
                    tradeDtTm = DateTime.SpecifyKind(parsedDt, DateTimeKind.Utc);

                // ── Structured row (descriptive column names) ─────────────────
                var tradeBook = new FoTradeBook
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradeDate = request.TradingDate,
                    Segment = f[2].Trim(),
                    Exchange = request.Exchange,
                    UniqueTradeId = trade.UniqueTradeId,
                    ClearingMemberId = f[5].Trim(),
                    BrokerId = trade.TradngMmbId,
                    InstrumentId = finInstrmId,
                    Symbol = trade.TckrSymb,
                    InstrumentName = trade.FinInstrmNm,
                    ContractType = contractType,
                    UnderlyingSymbol = underlyingSymbol,
                    Isin = trade.Isin,
                    ExpiryDate = expiryDate,
                    StrikePrice = trade.StrkPric,
                    OptionType = optionType,
                    LotSize = lotSize,
                    ClientType = trade.ClntTp,
                    ClientCode = clntId,
                    CtclId = trade.CtclId,
                    OriginalClientId = trade.OrgnlCtdnPtcptId,
                    ClientId = clientId,
                    ClientName = clientName,
                    ClientStateCode = clientStateCode,
                    Side = trade.BuySellInd,
                    TradeDateTime = tradeDtTm,
                    TradeStatus = trade.RptdTxSts,
                    OrderRef = trade.OrdrRef,
                    MarketType = trade.MktTpandId,
                    Remarks = f.Length > 41 ? NullIfBlank(f[41]) : null,
                    Quantity = tradQty,
                    NumberOfLots = trade.NumLots,
                    Price = pric,
                    TradeValue = trade.TradeValue,
                    SettlementType = trade.SttlmTp,
                    SettlementTransactionId = trade.SctiesSttlmTxId
                };

                chunk.Add(trade);
                chunkBook.Add(tradeBook);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _tradeRepo.AddRangeAsync(chunk, cancellationToken);
                    await _tradeBookRepo.AddRangeAsync(chunkBook, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _unitOfWork.ClearTracking();
                    chunk.Clear();
                    chunkBook.Clear();
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNum, ex.Message));
                logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = ex.Message, RawData = line[..Math.Min(line.Length, 1000)] });
                skipped++;
                created = Math.Max(0, created - 1);
            }
        }

        if (chunk.Count > 0)
        {
            await _tradeRepo.AddRangeAsync(chunk, cancellationToken);
            await _tradeBookRepo.AddRangeAsync(chunkBook, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _unitOfWork.ClearTracking();
        }

        if (logs.Count > 0)
        {
            await _logRepo.AddRangeAsync(logs, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var batchToUpdate = await _batchRepo.GetByIdAsync(batch.BatchId, cancellationToken);
        if (batchToUpdate != null)
        {
            batchToUpdate.Status = FoImportStatus.Completed;
            batchToUpdate.TotalRows = rowNum;
            batchToUpdate.CreatedRows = created;
            batchToUpdate.SkippedRows = skipped;
            batchToUpdate.ErrorRows = errors.Count;
            batchToUpdate.CompletedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ImportResultDto(created, skipped, errors);
    }

    internal static DateOnly? ParseExpiryDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Trim() == "0") return null;

        var v = raw.Trim();
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var ns = System.Globalization.DateTimeStyles.None;

        // NSE: Unix epoch MILLISECONDS — exactly 13 digits (e.g. "1743446400000")
        if (v.Length == 13 && long.TryParse(v, out var ms))
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime);

        // NSE: Unix epoch SECONDS — exactly 10 digits (e.g. "1743446400")
        if (v.Length == 10 && long.TryParse(v, out var sec))
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime);

        // BSE / common string formats
        if (DateOnly.TryParseExact(v, "dd-MMM-yyyy", ci, ns, out var d1)) return d1;  // 31-MAR-2026
        if (DateOnly.TryParseExact(v, "dd-MMM-yy",   ci, ns, out var d2)) return d2;  // 31-MAR-26
        if (DateOnly.TryParseExact(v, "yyyy-MM-dd",   ci, ns, out var d3)) return d3;  // 2026-03-31
        if (DateOnly.TryParseExact(v, "dd-MM-yyyy",   ci, ns, out var d4)) return d4;  // 31-03-2026
        if (DateOnly.TryParseExact(v, "dd/MM/yyyy",   ci, ns, out var d5)) return d5;  // 31/03/2026
        if (v.Length == 8 &&
            DateOnly.TryParseExact(v, "yyyyMMdd",     ci, ns, out var d6)) return d6;  // 20260331

        return null;
    }

    /// <summary>
    /// Normalises the exchange instrument-type code to NSE-style short codes.
    /// NSE already sends FUTIDX/FUTSTK/OPTIDX/OPTSTK; BSE sends IDF/STF/IDO/STO.
    /// </summary>
    internal static string MapInstrumentType(string finInstrmTp) => finInstrmTp.ToUpperInvariant() switch
    {
        "IDF"    => "FUTIDX",   // BSE Index Future
        "STF"    => "FUTSTK",   // BSE Stock Future
        "IDO"    => "OPTIDX",   // BSE Index Option
        "STO"    => "OPTSTK",   // BSE Stock Option
        "FUTIDX" => "FUTIDX",
        "FUTSTK" => "FUTSTK",
        "OPTIDX" => "OPTIDX",
        "OPTSTK" => "OPTSTK",
        _        => finInstrmTp.ToUpperInvariant()
    };

    /// <summary>Returns CE | PE | FX — blank option type means it is a futures contract.</summary>
    internal static string ResolveOptionType(string optnTp) =>
        string.IsNullOrWhiteSpace(optnTp) ? "FX" : optnTp.Trim().ToUpperInvariant();

    private static string? NullIfBlank(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
