using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record ImportFoSttCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,        // NFO (NCL clears both NFO and BFO)
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportFoSttCommandHandler : IRequestHandler<ImportFoSttCommand, ImportResultDto>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IRepository<FoStt> _sttRepo;
    private readonly IRepository<FoSttLedger> _sttLedgerRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoSttCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoStt> sttRepo,
        IRepository<FoSttLedger> sttLedgerRepo,
        IRepository<FoFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _sttRepo = sttRepo;
        _sttLedgerRepo = sttLedgerRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoSttCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.Stt &&
            b.Exchange == request.Exchange &&
            b.TradingDate == request.TradingDate &&
            b.Status == FoImportStatus.Completed,
            cancellationToken);

        if (existing != null)
            throw new DuplicateImportException(existing.BatchId);

        var batch = new FoFileImportBatch
        {
            BatchId = Guid.NewGuid(),
            TenantId = tenantId,
            FileType = FoFileType.Stt,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = FoImportStatus.Processing,
            TriggerSource = Enum.TryParse<FoTriggerSource>(request.TriggerSource, out var src) ? src : FoTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };
        await _batchRepo.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var clients = await _clientRepo.GetAllAsync(cancellationToken);
        var clientMap = clients
            .Where(c => !string.IsNullOrEmpty(c.ClientCode))
            .ToDictionary(
                c => c.ClientCode!,
                c => (c.ClientId, c.ClientName, c.StateCode),
                StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        var chunk = new List<FoStt>();
        var chunkLedger = new List<FoSttLedger>();
        int rowNum = 0, created = 0, skipped = 0;

        // File columns (0-indexed):
        // 0=RptHdr,1=Sgmt,2=Src,3=TradDt,4=ClctnDt,5=DueDt,6=SttlmTp,7=SctiesSttlmTxId,
        // 8=ClrMmbId,9=BrkrOrCtdnPtcptId,10=ClntId,11=TckrSymb,12=SctySrs,13=FinInstrmId,
        // 14=FinInstrmTp,15=ISIN,16=XpryDt,17=OptnTp,18=StrkPric,19=SttlmPric,
        // 20=TtlBuyTradgVol,21=TtlBuyTrfVal,22=TtlSellTradgVol,23=TtlSellTrfVal,
        // 34=TaxblSellFutrsVal,35=TaxblSellOptnVal,36=OptnExrcQty,37=OptnExrcVal,
        // 38=TaxblExrcVal,39=FutrsTtlTaxs,40=OptnTtlTaxs,43=TtlTaxs
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
                if (f.Length < 20)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var rptHdr = f[0].Trim();

                // Only process client-level rows (RptHdr == "30")
                if (rptHdr != "30")
                {
                    skipped++;
                    continue;
                }

                var clntId = f[10].Trim();
                if (string.IsNullOrEmpty(clntId))
                {
                    skipped++;
                    continue;
                }

                Guid? clientId = null;
                string? clientName = null;
                string? clientStateCode = null;
                if (clientMap.TryGetValue(clntId, out var clientInfo))
                {
                    clientId = clientInfo.ClientId;
                    clientName = clientInfo.ClientName;
                    clientStateCode = clientInfo.StateCode;
                }
                else
                {
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Client '{clntId}' not found in master" });
                }

                var finInstrmTp = f[14].Trim();
                var optnTp = f[17].Trim();
                var xpryDt = f[16].Trim();
                var expiryDate = ImportFoTradeFileCommandHandler.ParseExpiryDate(xpryDt);
                var contractType = ImportFoTradeFileCommandHandler.MapInstrumentType(finInstrmTp);
                var optionType = ImportFoTradeFileCommandHandler.ResolveOptionType(optnTp);
                var strikePrice = decimal.TryParse(f[18].Trim(), out var sp) ? sp : 0m;
                var settlementPrice = decimal.TryParse(f[19].Trim(), out var sttl) ? sttl : 0m;

                // ── Staging row ───────────────────────────────────────────────
                var row = new FoStt
                {
                    SttRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    RptHdr = rptHdr,
                    TradDt = request.TradingDate,
                    Sgmt = f[1].Trim(),
                    Src = f[2].Trim(),
                    Exchange = request.Exchange,
                    ClrMmbId = f[8].Trim(),
                    TradngMmbId = f[9].Trim(),
                    ClntId = clntId,
                    ClientId = clientId,
                    ClientName = clientName,
                    ClientStateCode = clientStateCode,
                    TckrSymb = f[11].Trim(),
                    FinInstrmTp = finInstrmTp,
                    Isin = f[15].Trim(),
                    XpryDt = xpryDt,
                    ExpiryDate = expiryDate,
                    OptnTp = optnTp,
                    StrkPric = strikePrice,
                    SttlmPric = settlementPrice,
                    TtlBuyTradgVol = long.TryParse(f[20].Trim(), out var bvol) ? bvol : 0,
                    TtlBuyTrfVal = decimal.TryParse(f[21].Trim(), out var bval) ? bval : 0,
                    TtlSellTradgVol = long.TryParse(f[22].Trim(), out var svol) ? svol : 0,
                    TtlSellTrfVal = decimal.TryParse(f[23].Trim(), out var sval) ? sval : 0,
                    TaxblSellFutrsVal = f.Length > 34 && decimal.TryParse(f[34].Trim(), out var tfv) ? tfv : 0,
                    TaxblSellOptnVal = f.Length > 35 && decimal.TryParse(f[35].Trim(), out var tov) ? tov : 0,
                    OptnExrcQty = f.Length > 36 && long.TryParse(f[36].Trim(), out var oeq) ? oeq : 0,
                    OptnExrcVal = f.Length > 37 && decimal.TryParse(f[37].Trim(), out var oev) ? oev : 0,
                    TaxblExrcVal = f.Length > 38 && decimal.TryParse(f[38].Trim(), out var tev) ? tev : 0,
                    FutrsTtlTaxs = f.Length > 39 && decimal.TryParse(f[39].Trim(), out var ftt) ? ftt : 0,
                    OptnTtlTaxs = f.Length > 40 && decimal.TryParse(f[40].Trim(), out var ott) ? ott : 0,
                    TtlTaxs = f.Length > 43 && decimal.TryParse(f[43].Trim(), out var tt) ? tt : 0
                };

                // ── Structured row ────────────────────────────────────────────
                var ledger = new FoSttLedger
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradeDate = request.TradingDate,
                    Exchange = request.Exchange,
                    ClearingMemberId = row.ClrMmbId,
                    BrokerId = row.TradngMmbId,
                    ClientCode = clntId,
                    ClientId = clientId,
                    ClientName = clientName,
                    ClientStateCode = clientStateCode,
                    Symbol = row.TckrSymb,
                    ContractType = contractType,
                    Isin = row.Isin,
                    ExpiryDate = expiryDate,
                    StrikePrice = strikePrice,
                    OptionType = optionType,
                    SettlementPrice = settlementPrice,
                    TotalBuyQty = row.TtlBuyTradgVol,
                    TotalBuyValue = row.TtlBuyTrfVal,
                    TotalSellQty = row.TtlSellTradgVol,
                    TotalSellValue = row.TtlSellTrfVal,
                    TaxableSellFuturesValue = row.TaxblSellFutrsVal,
                    TaxableSellOptionValue = row.TaxblSellOptnVal,
                    OptionExerciseQty = row.OptnExrcQty,
                    OptionExerciseValue = row.OptnExrcVal,
                    TaxableExerciseValue = row.TaxblExrcVal,
                    FuturesStt = row.FutrsTtlTaxs,
                    OptionsStt = row.OptnTtlTaxs,
                    TotalStt = row.TtlTaxs
                };

                chunk.Add(row);
                chunkLedger.Add(ledger);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _sttRepo.AddRangeAsync(chunk, cancellationToken);
                    await _sttLedgerRepo.AddRangeAsync(chunkLedger, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _unitOfWork.ClearTracking();
                    chunk.Clear();
                    chunkLedger.Clear();
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
            await _sttRepo.AddRangeAsync(chunk, cancellationToken);
            await _sttLedgerRepo.AddRangeAsync(chunkLedger, cancellationToken);
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
}
