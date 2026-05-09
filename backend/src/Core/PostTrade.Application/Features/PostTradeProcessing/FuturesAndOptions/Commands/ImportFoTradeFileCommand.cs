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
    private readonly IRepository<FoTradeDate> _tradeDateRepo;
    private readonly IRepository<FoTradeBook> _tradeBookRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IRepository<FoContract> _contractRepo;
    private readonly IRepository<ExchangeSegment> _exchangeSegmentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int SaveChunkSize = 5000;

    public ImportFoTradeFileCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoTrade> tradeRepo,
        IRepository<FoTradeDate> tradeDateRepo,
        IRepository<FoTradeBook> tradeBookRepo,
        IRepository<FoFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IRepository<FoContract> contractRepo,
        IRepository<ExchangeSegment> exchangeSegmentRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _tradeRepo = tradeRepo;
        _tradeDateRepo = tradeDateRepo;
        _tradeBookRepo = tradeBookRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _contractRepo = contractRepo;
        _exchangeSegmentRepo = exchangeSegmentRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoTradeFileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Prerequisite: curated FoContract must exist for this exchange/date
        var hasContract = await _contractRepo.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
            cancellationToken);
        if (hasContract == null)
            throw new PrerequisiteNotMetException(
                $"FO Contract Master missing for Exchange '{request.Exchange}' on {request.TradingDate:yyyy-MM-dd}. Import the contract master file first.");

        // ExchangeSegment lookup for GlobalExchange (IntraOp: NCL→NFO, ICCL→BFO)
        var segment = await _exchangeSegmentRepo.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.ExchangeSegmentCode == request.Exchange,
            cancellationToken);
        var globalExchange = segment?.GlobalExchangeCode ?? request.Exchange;

        // Re-import: delete previous data and reuse the batch
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
            await _unitOfWork.ExecuteDeleteAsync<FoTradeDate>(
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

        // ── Lookup tables loaded once ─────────────────────────────────────────
        var clients = await _clientRepo.GetAllAsync(cancellationToken);
        var clientMap = clients
            .Where(c => !string.IsNullOrEmpty(c.ClientCode))
            .ToDictionary(c => c.ClientCode!, c => (c.ClientId, c.ClientName, c.StateCode),
                StringComparer.OrdinalIgnoreCase);

        // Load contracts keyed by (InstrumentType|Symbol|ExpiryDate|OptionType) — matching
        // CFORise UpdateFmultiplierOrFo_Unit join: EXPIRYDATE, SYMBOL, OPTTYPE, INSTRUMENT_TYPE, EXCHANGE=Exch_New
        var contracts = await _contractRepo.FindAsync(
            c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
            cancellationToken);
        var contractMap = contracts
            .GroupBy(c => $"{c.InstrumentType}|{c.Symbol}|{c.ExpiryDate:yyyy-MM-dd}|{c.OptionType}",
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // ── Phase 1: Parse entire file into memory — zero DB calls ────────────
        // File columns (NSE/BSE FO trade file — 46 columns):
        // 0=TradDt,1=BizDt,2=Sgmt,3=Src,4=Xchg,5=ClrMmbId,6=Brkr,7=FinInstrmTp,8=FinInstrmId,
        // 9=ISIN,10=TckrSymb,11=SctySrs,12=XpryDt,13=FininstrmActlXpryDt,14=StrkPric,15=OptnTp,
        // 16=FinInstrmNm,17=ClntTp,18=ClntId,19=FullyExctdConfSnt,20=OrgnlCtdnPtcptId,21=CtdnPtcptId,
        // 22=SttlmTp,23=SctiesSttlmTxId,24=BuySellInd,25=TradQty,26=NewBrdLotQty,27=Pric,28=UnqTradIdr,
        // 29=RptdTxSts,30=TradDtTm,31=UpdDt,32=OrdrRef,33=OrdrDtTm,34=InstgUsr,35=CtclId,
        // 36=TradRegnOrgn,37=OrdrTp,38=BlckDealInd,39=SttlmCycl,40=MktTpandId,41=Rmks,42-45=Rsvd
        var allTrades = new List<FoTrade>();
        var allTradeDates = new List<FoTradeDate>();
        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        // Dedup guard: (UniqueTradeId|ClientCode|Side) — prevents duplicate key on FoTradeDate.
        // Same UniqueTradeId CAN appear for different clients (intra-broker both legs) — that is valid.
        // But the same client on the same side of the same trade must appear only once.
        var seenTradeDateKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int rowNum = 0, skipped = 0;

        using var reader = new StreamReader(request.FileStream);
        string? line;
        bool isHeader = true;

        while ((line = await reader.ReadLineAsync(CancellationToken.None)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (isHeader) { isHeader = false; continue; }

            rowNum++;
            try
            {
                var f = CsvParser.ParseLine(line);
                if (f.Length < 29)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var uniqueTradeId = f[28].Trim();
                var rptdTxSts     = f.Length > 29 ? f[29].Trim() : string.Empty;
                var finInstrmId   = f[8].Trim();
                var clntId        = f[18].Trim();
                var tckrSymb      = f[10].Trim();
                var finInstrmTp   = f[7].Trim();
                var optnTp        = f[15].Trim();
                var contractType  = MapInstrumentType(finInstrmTp);
                var optionType    = ResolveOptionType(optnTp);
                var xpryDt        = f[12].Trim();
                var expiryDate    = ParseExpiryDate(xpryDt);
                var tradQty       = long.TryParse(f[25].Trim(), out var qty) ? qty : 0L;
                var pric          = decimal.TryParse(f[27].Trim(), out var p) ? p : 0m;
                var strikePrice   = decimal.TryParse(f[14].Trim(), out var sp) ? sp : 0m;
                var orgnlCtdn     = f.Length > 20 ? NullIfBlank(f[20]) : null;
                var tradDtTmRaw   = f.Length > 30 ? NullIfBlank(f[30]) : null;
                var ctclId        = f.Length > 35 ? NullIfBlank(f[35]) : null;

                // Contract enrichment — lookup by (InstrumentType|Symbol|ExpiryDate|OptionType)
                // Matches CFORise UpdateFmultiplierOrFo_Unit join columns
                var contractKey = expiryDate.HasValue
                    ? $"{contractType}|{tckrSymb}|{expiryDate.Value:yyyy-MM-dd}|{optionType}"
                    : null;
                FoContract? contract = null;
                if (contractKey != null) contractMap.TryGetValue(contractKey, out contract);
                var lotSize          = contract?.LotSize ?? 0L;
                var fMultiplier      = contract?.FMultiplier ?? 1m;
                var contractName     = contract?.ContractName ?? string.Empty;
                var underlyingSymbol = contract?.UnderlyingSymbol ?? string.Empty;

                if (contract == null)
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Info", Message = $"Contract not found: InstrType='{contractType}' Symbol='{tckrSymb}' Expiry='{expiryDate?.ToString("yyyy-MM-dd") ?? "null"}' OptionType='{optionType}' — LotSize/FMultiplier defaulted" });

                // Client enrichment
                string? clientName = null, clientStateCode = null;
                Guid? clientId = null;
                if (string.IsNullOrWhiteSpace(clntId))
                {
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Blank client code in file at row {rowNum}" });
                }
                else if (clientMap.TryGetValue(clntId, out var ci))
                {
                    clientId = ci.ClientId; clientName = ci.ClientName; clientStateCode = ci.StateCode;
                }
                else
                {
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Info", Message = $"Client '{clntId}' not mapped in client master — enrichment skipped" });
                }

                // ── Staging row (ALL rows including cancelled — audit trail) ──
                allTrades.Add(new FoTrade
                {
                    TradeRowId        = Guid.NewGuid(),
                    BatchId           = batch.BatchId,
                    TenantId          = tenantId,
                    UniqueTradeId     = uniqueTradeId,
                    TradDt            = request.TradingDate,
                    Sgmt              = f[2].Trim(),
                    Src               = f[3].Trim(),
                    Exchange          = request.Exchange,
                    TradngMmbId       = f[6].Trim(),
                    FinInstrmTp       = finInstrmTp,
                    FinInstrmId       = finInstrmId,
                    Isin              = f[9].Trim(),
                    TckrSymb          = tckrSymb,
                    XpryDt            = xpryDt,
                    ExpiryDate        = expiryDate,
                    StrkPric          = strikePrice,
                    OptnTp            = optnTp,
                    FinInstrmNm       = f[16].Trim(),
                    InstrumentType    = contractType,
                    UnderlyingSymbol  = underlyingSymbol,
                    LotSize           = lotSize,
                    ClntTp            = f[17].Trim(),
                    ClntId            = clntId,
                    CtclId            = ctclId,
                    OrgnlCtdnPtcptId  = orgnlCtdn,
                    ClientId          = clientId,
                    ClientName        = clientName,
                    ClientStateCode   = clientStateCode,
                    SttlmTp           = f[22].Trim(),
                    SctiesSttlmTxId   = f[23].Trim(),
                    BuySellInd        = f[24].Trim(),
                    TradQty           = tradQty,
                    NewBrdLotQty      = long.TryParse(f[26].Trim(), out var lot) ? lot : 0,
                    Pric              = pric,
                    TradeValue        = tradQty * pric * fMultiplier,
                    NumLots           = lotSize > 0 ? Math.Round((decimal)tradQty / lotSize, 4) : 0,
                    TradDtTm          = tradDtTmRaw,
                    RptdTxSts         = rptdTxSts,
                    OrdrRef           = f.Length > 32 ? NullIfBlank(f[32]) : null,
                    SttlmCycl         = f.Length > 39 ? NullIfBlank(f[39]) : null,
                    MktTpandId        = f.Length > 40 ? NullIfBlank(f[40]) : null,
                });

                // ── Date table — skip cancelled, then dedup on (UniqueTradeId|Client|Side) ──
                if (string.Equals(rptdTxSts, "CN", StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    continue;
                }

                var tradeDateKey = $"{uniqueTradeId}|{clntId}|{f[24].Trim()}";
                if (!seenTradeDateKeys.Add(tradeDateKey))
                {
                    // Same trade leg for same client already seen — file contains duplicate row
                    skipped++;
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Info", Message = $"Duplicate trade row skipped: UniqueTradeId='{uniqueTradeId}' Client='{clntId}' Side='{f[24].Trim()}'" });
                    continue;
                }

                // Parse trade datetime to UTC for Npgsql timestamptz
                DateTime? tradeDateTime = null;
                if (!string.IsNullOrWhiteSpace(tradDtTmRaw) &&
                    DateTime.TryParse(tradDtTmRaw, null,
                        System.Globalization.DateTimeStyles.AssumeUniversal |
                        System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsedDt))
                    tradeDateTime = DateTime.SpecifyKind(parsedDt, DateTimeKind.Utc);

                // TrnSlNo is 0 here — will be assigned in bulk from the sequence below
                allTradeDates.Add(new FoTradeDate
                {
                    Id                      = Guid.NewGuid(),
                    BatchId                 = batch.BatchId,
                    TenantId                = tenantId,
                    TradeDate               = request.TradingDate,
                    Exchange                = request.Exchange,
                    GlobalExchange          = globalExchange,
                    Segment                 = f[2].Trim(),
                    UniqueTradeId           = uniqueTradeId,
                    TrdType                 = "11",
                    ClearingMemberId        = f[5].Trim(),
                    TradingMemberId         = f[6].Trim(),
                    InstrumentType          = contractType,
                    Symbol                  = tckrSymb,
                    ContractName            = contractName,
                    InstrumentId            = finInstrmId,
                    ExpiryDate              = expiryDate,
                    StrikePrice             = strikePrice,
                    OptionType              = optionType,
                    UnderlyingSymbol        = underlyingSymbol,
                    Isin                    = NullIfBlank(f[9]),
                    LotSize                 = lotSize,
                    FMultiplier             = fMultiplier,
                    ClientCode              = clntId,
                    OriginalClientCode      = orgnlCtdn ?? string.Empty,
                    ClientType              = f[17].Trim(),
                    CtclId                  = ctclId,
                    ClientId                = clientId,
                    ClientName              = clientName,
                    ClientStateCode         = clientStateCode,
                    IsCustodianTrade        = f.Length > 19 && string.Equals(f[19].Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                    Side                    = f[24].Trim(),
                    Quantity                = tradQty,
                    NumberOfLots            = lotSize > 0 ? Math.Round((decimal)tradQty / lotSize, 4) : 0,
                    Price                   = pric,
                    NetPrice                = pric,
                    TradeValue              = tradQty * pric * fMultiplier,
                    TradeDateTime           = tradeDateTime,
                    TradeStatus             = NullIfBlank(rptdTxSts),
                    OrderRef                = f.Length > 32 ? NullIfBlank(f[32]) : null,
                    MarketType              = "01",  // CFORise hardcodes mkt_type='01'
                    BookType                = "RL",
                    BookTypeName            = "RL",
                    SettlementType          = NullIfBlank(f[22]),
                    SettlementTransactionId = NullIfBlank(f[23]),
                    Remarks                 = f.Length > 41 ? NullIfBlank(f[41]) : null,
                    FileName                = request.FileName
                });
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNum, ex.Message));
                logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = ex.Message, RawData = line[..Math.Min(line.Length, 1000)] });
                skipped++;
            }
        }

        // ── Phase 2: Pre-generate ALL TrnSlNo values in ONE round-trip ────────
        // Equivalent of CFORise Oracle SYSDBSEQUENCE bulk nextval fetch.
        // Fetching all values upfront avoids N individual nextval() calls during inserts.
        var trnSlNos = await _unitOfWork.GetNextSequenceValuesAsync(
            "post_trade.fo_trn_slno_seq", allTradeDates.Count, CancellationToken.None);

        for (int i = 0; i < allTradeDates.Count; i++)
            allTradeDates[i].TrnSlNo = trnSlNos[i];

        // ── Phase 3: Build FoTradeBook from FoTradeDate (TrnSlNo already known) ─
        var allTradeBooks = allTradeDates.Select(td => new FoTradeBook
        {
            Id                      = Guid.NewGuid(),
            BatchId                 = td.BatchId,
            TenantId                = td.TenantId,
            TrnSlNo                 = td.TrnSlNo,
            TrdType                 = td.TrdType,
            TradeDate               = td.TradeDate,
            Segment                 = td.Segment,
            Exchange                = td.Exchange,
            GlobalExchange          = td.GlobalExchange,
            UniqueTradeId           = td.UniqueTradeId,
            ClearingMemberId        = td.ClearingMemberId,
            BrokerId                = td.TradingMemberId,
            InstrumentId            = td.InstrumentId,
            Symbol                  = td.Symbol,
            InstrumentName          = td.ContractName,
            ContractType            = td.InstrumentType,
            UnderlyingSymbol        = td.UnderlyingSymbol,
            Isin                    = td.Isin ?? string.Empty,
            ExpiryDate              = td.ExpiryDate,
            StrikePrice             = td.StrikePrice,
            OptionType              = td.OptionType,
            LotSize                 = td.LotSize,
            FMultiplier             = td.FMultiplier,
            ClientType              = td.ClientType,
            ClientCode              = td.ClientCode,
            CtclId                  = td.CtclId,
            OriginalClientId        = NullIfBlank(td.OriginalClientCode),
            ClientId                = td.ClientId,
            ClientName              = td.ClientName,
            ClientStateCode         = td.ClientStateCode,
            IsCustodianTrade        = td.IsCustodianTrade,
            Side                    = td.Side,
            Quantity                = td.Quantity,
            NumberOfLots            = td.NumberOfLots,
            Price                   = td.Price,
            NetPrice                = td.NetPrice,
            TradeValue              = td.TradeValue,
            TradeDateTime           = td.TradeDateTime,
            TradeStatus             = td.TradeStatus,
            OrderRef                = td.OrderRef,
            MarketType              = td.MarketType,
            BookType                = td.BookType,
            BookTypeName            = td.BookTypeName,
            SettlementType          = td.SettlementType ?? string.Empty,
            SettlementTransactionId = td.SettlementTransactionId ?? string.Empty,
            Remarks                 = td.Remarks
        }).ToList();

        // ── Save: each table in large chunks — far fewer round-trips than before ─
        await SaveChunkedAsync(allTrades, _tradeRepo, SaveChunkSize);
        await SaveChunkedAsync(allTradeDates, _tradeDateRepo, SaveChunkSize);
        await SaveChunkedAsync(allTradeBooks, _tradeBookRepo, SaveChunkSize);

        if (logs.Count > 0)
        {
            await SaveChunkedAsync(logs, _logRepo, SaveChunkSize);
        }

        var batchToUpdate = await _batchRepo.GetByIdAsync(batch.BatchId, CancellationToken.None);
        if (batchToUpdate != null)
        {
            batchToUpdate.Status = FoImportStatus.Completed;
            batchToUpdate.TotalRows = rowNum;
            batchToUpdate.CreatedRows = allTradeDates.Count;
            batchToUpdate.SkippedRows = skipped;
            batchToUpdate.ErrorRows = errors.Count;
            batchToUpdate.CompletedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        return new ImportResultDto(allTradeDates.Count, skipped, errors);
    }

    private async Task SaveChunkedAsync<T>(List<T> items, IRepository<T> repo, int chunkSize) where T : class
    {
        for (int i = 0; i < items.Count; i += chunkSize)
        {
            var chunk = items.GetRange(i, Math.Min(chunkSize, items.Count - i));
            await repo.AddRangeAsync(chunk, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            _unitOfWork.ClearTracking();
        }
    }

    internal static DateOnly? ParseExpiryDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Trim() == "0") return null;

        var v = raw.Trim();
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var ns = System.Globalization.DateTimeStyles.None;

        if (v.Length == 13 && long.TryParse(v, out var ms))
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime);

        if (v.Length == 10 && long.TryParse(v, out var sec))
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime);

        if (DateOnly.TryParseExact(v, "dd-MMM-yyyy", ci, ns, out var d1)) return d1;
        if (DateOnly.TryParseExact(v, "dd-MMM-yy",   ci, ns, out var d2)) return d2;
        if (DateOnly.TryParseExact(v, "yyyy-MM-dd",   ci, ns, out var d3)) return d3;
        if (DateOnly.TryParseExact(v, "dd-MM-yyyy",   ci, ns, out var d4)) return d4;
        if (DateOnly.TryParseExact(v, "dd/MM/yyyy",   ci, ns, out var d5)) return d5;
        if (v.Length == 8 && DateOnly.TryParseExact(v, "yyyyMMdd", ci, ns, out var d6)) return d6;

        return null;
    }

    internal static string MapInstrumentType(string finInstrmTp) => finInstrmTp.ToUpperInvariant() switch
    {
        // NSE instrument type codes
        "IDF"    => "FUTIDX",
        "STF"    => "FUTSTK",
        "IDO"    => "OPTIDX",
        "STO"    => "OPTSTK",
        // BSE EQD instrument type codes (SO=Stock Option, SF=Stock Future, IO=Index Option, IF=Index Future)
        "SO"     => "OPTSTK",
        "SF"     => "FUTSTK",
        "IO"     => "OPTIDX",
        "IF"     => "FUTIDX",
        "FUTIDX" => "FUTIDX",
        "FUTSTK" => "FUTSTK",
        "OPTIDX" => "OPTIDX",
        "OPTSTK" => "OPTSTK",
        _        => finInstrmTp.ToUpperInvariant()
    };

    internal static string ResolveOptionType(string optnTp)
    {
        // Matches CFORise: CASE WHEN optiontype='XX' THEN 'FX' WHEN NVL(optiontype,'1')='1' THEN 'FX' ELSE optiontype END
        if (string.IsNullOrWhiteSpace(optnTp)) return "FX";
        var v = optnTp.Trim().ToUpperInvariant();
        return (v == "XX" || v == "1") ? "FX" : v;
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
