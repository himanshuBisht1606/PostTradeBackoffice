using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record ImportFoContractMasterCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,        // NFO | BFO
    string FileName
) : IRequest<ImportResultDto>;

public class ImportFoContractMasterCommandHandler : IRequestHandler<ImportFoContractMasterCommand, ImportResultDto>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IRepository<FoContractMaster> _contractRepo;
    private readonly IRepository<FoContract> _foContractRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoContractMasterCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoContractMaster> contractRepo,
        IRepository<FoContract> foContractRepo,
        IRepository<FoFileImportLog> logRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _contractRepo = contractRepo;
        _foContractRepo = foContractRepo;
        _logRepo = logRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoContractMasterCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Re-import: delete existing staging + curated rows for this exchange/date
        var existingBatch = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.ContractMaster &&
            b.Exchange == request.Exchange &&
            b.TradingDate == request.TradingDate &&
            b.Status == FoImportStatus.Completed,
            cancellationToken);

        if (existingBatch != null)
        {
            await _unitOfWork.ExecuteDeleteAsync<FoContractMaster>(
                c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
                cancellationToken);
            await _unitOfWork.ExecuteDeleteAsync<FoContract>(
                c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
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
            FileType = FoFileType.ContractMaster,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = FoImportStatus.Processing,
            TriggerSource = FoTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };

        if (existingBatch == null)
        {
            await _batchRepo.AddAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        var stagingChunk = new List<FoContractMaster>();
        var curatedChunk = new List<FoContract>();
        int rowNum = 0, created = 0, skipped = 0;

        // NFO / BFO contract file column layout (comma-delimited, identical for both exchanges):
        // 0=FinInstrmId, 1=UndrlygFinInstrmId, 2=FinInstrmNm(type code), 3=TckrSymb, 4=XpryDt,
        // 5=StrkPric, 6=OptnTp, 7=PrtdToTrad, 8=MinLot, 9=NewBrdLotQty, 10=BidIntrvl(TickSize),
        // 18=StockNm(NSE: full contract name), 19=SttlmMtd(NSE: C/D),
        // 20=BasePric(NSE only), 27=MktTpAndId, 60=OptnExrcStyle,
        // 71=Mltplr (FMultiplier / CMULTIPLIER), 110=ISIN
        bool isNse = request.Exchange == "NFO";
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
                if (f.Length < 10)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var xpryDt = f[4].Trim();
                var expiryDate = ImportFoTradeFileCommandHandler.ParseExpiryDate(xpryDt);

                // Skip spot / underlying instruments — no expiry = not a tradeable derivative contract
                // (equivalent of CFORise: DELETE WHERE XpryDt IS NULL OR XpryDt IN ('-1','0','0001-01-01'))
                if (expiryDate == null)
                {
                    skipped++;
                    continue;
                }

                var finInstrmTp = ImportFoTradeFileCommandHandler.MapInstrumentType(f[2].Trim());
                var optnTp = f[6].Trim();
                var optionType = ImportFoTradeFileCommandHandler.ResolveOptionType(optnTp);
                // StrkPric in the NSE/BSE contract file is in paise (×100).
                // CFORise divides by 100: CASE WHEN StrkPric='-1' THEN 0 ELSE TO_NUMBER(StrkPric)/100 END
                var strkPricRaw = decimal.TryParse(f[5].Trim(), out var sp) ? sp : 0m;
                var strkPric = (strkPricRaw == -1m) ? 0m : strkPricRaw / 100m;  // rupees (as in CONTRACTS.STRIKE)
                var minLot = long.TryParse(f[8].Trim(), out var ml) ? ml : 0L;
                var newBrdLot = long.TryParse(f[9].Trim(), out var lot) ? lot : 0L;
                var lotSize = minLot > 0 ? minLot : newBrdLot;

                // FMultiplier = Mltplr at f[71]; default 1 if missing or blank
                // (CFORise: CASE WHEN NVL(Mltplr,'1')='1' THEN '1' ELSE Mltplr END)
                decimal fMultiplier = 1m;
                if (f.Length > 71 && decimal.TryParse(f[71].Trim(), out var mltplr) && mltplr > 0)
                    fMultiplier = mltplr;

                // Full contract name: NSE at f[18], BSE at f[19]
                var fullContractName = isNse
                    ? (f.Length > 18 ? f[18].Trim() : string.Empty)
                    : (f.Length > 19 ? f[19].Trim() : string.Empty);

                var sttlmMtd = isNse && f.Length > 19 ? f[19].Trim() : string.Empty;

                decimal? basePric = null;
                if (isNse && f.Length > 20 && decimal.TryParse(f[20].Trim(), out var bp) && bp > 0)
                    basePric = bp;

                var finInstrmId = f[0].Trim();
                var tckrSymb = f[3].Trim();

                // Derived contract name key: UPPER(InstrType + Symbol + ExpiryDate_DDMONYYYY)
                // Equivalent of CFORise CONTNAME used for deduplication and ledger reference
                var contractName = BuildContractName(finInstrmTp, tckrSymb, expiryDate.Value);

                // ── Phase 1: Staging row (FoContractMaster) ───────────────────────────
                stagingChunk.Add(new FoContractMaster
                {
                    ContractRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradingDate = request.TradingDate,
                    Exchange = request.Exchange,
                    FinInstrmId = finInstrmId,
                    UndrlygFinInstrmId = f[1].Trim(),
                    FinInstrmNm = fullContractName,
                    TckrSymb = tckrSymb,
                    XpryDt = xpryDt,
                    ExpiryDate = expiryDate,
                    StrkPric = strkPricRaw,   // raw paise value from file (staging keeps original)
                    OptnTp = optnTp,
                    MinLot = minLot,
                    NewBrdLotQty = newBrdLot,
                    TickSize = f.Length > 10 && decimal.TryParse(f[10].Trim(), out var ts) ? ts : 0,
                    StockNm = isNse ? fullContractName : (f.Length > 18 ? f[18].Trim() : string.Empty),
                    SttlmMtd = sttlmMtd,
                    BasePric = basePric,
                    MktTpAndId = f.Length > 27 ? NullIfBlank(f[27]) : null,
                    OptnExrcStyle = f.Length > 60 ? NullIfBlank(f[60]) : null,
                    Isin = f.Length > 110 ? NullIfBlank(f[110]) : null,
                    FinInstrmTp = finInstrmTp,
                    FMultiplier = fMultiplier
                });

                // ── Phase 2: Curated row (FoContract) ─────────────────────────────────
                curatedChunk.Add(new FoContract
                {
                    ContractId = Guid.NewGuid(),
                    TenantId = tenantId,
                    SourceBatchId = batch.BatchId,
                    Exchange = request.Exchange,
                    TradingDate = request.TradingDate,
                    InstrumentType = finInstrmTp,
                    Symbol = tckrSymb,
                    ContractName = contractName,
                    ExpiryDate = expiryDate.Value,
                    StrikePrice = strkPric,
                    OptionType = optionType,
                    LotSize = lotSize,
                    FMultiplier = fMultiplier,
                    FinInstrmId = finInstrmId,
                    UnderlyingSymbol = f[1].Trim(),
                    Isin = f.Length > 110 ? NullIfBlank(f[110]) : null,
                    TickSize = f.Length > 10 && decimal.TryParse(f[10].Trim(), out var ts2) ? ts2 : 0,
                    SttlmMtd = NullIfBlank(sttlmMtd)
                });

                created++;

                if (stagingChunk.Count >= BatchSize)
                {
                    await _contractRepo.AddRangeAsync(stagingChunk, cancellationToken);
                    await _foContractRepo.AddRangeAsync(curatedChunk, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _unitOfWork.ClearTracking();
                    stagingChunk.Clear();
                    curatedChunk.Clear();
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

        if (stagingChunk.Count > 0)
        {
            await _contractRepo.AddRangeAsync(stagingChunk, cancellationToken);
            await _foContractRepo.AddRangeAsync(curatedChunk, cancellationToken);
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

    /// <summary>
    /// Builds the canonical contract name key: UPPER(InstrumentType + Symbol + ExpiryDate_DDMONYYYY).
    /// Equivalent of CFORise CONTNAME — used for deduplication and ledger references.
    /// Example: "FUTIDXNIFTY27MAR2025", "OPTSTKRELIANCE27MAR2025"
    /// </summary>
    internal static string BuildContractName(string instrumentType, string symbol, DateOnly expiryDate)
        => $"{instrumentType}{symbol}{expiryDate:ddMMMyyyy}".ToUpperInvariant();

    private static string? NullIfBlank(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
