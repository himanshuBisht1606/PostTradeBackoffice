using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record ImportFoStampDutyCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportFoStampDutyCommandHandler : IRequestHandler<ImportFoStampDutyCommand, ImportResultDto>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IRepository<FoStampDuty> _stampRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoStampDutyCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoStampDuty> stampRepo,
        IRepository<FoFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _stampRepo = stampRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoStampDutyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.StampDuty &&
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
            FileType = FoFileType.StampDuty,
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
        var clientMap = clients.ToDictionary(
            c => c.ClientCode,
            c => (c.ClientId, c.ClientName, c.StateCode),
            StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        var chunk = new List<FoStampDuty>();
        int rowNum = 0, created = 0, skipped = 0;

        // File columns:
        // 0=RptHdr,1=Sgmt,2=Src,3=SttlmTp,4=SctiesSttlmTxId,5=ClrMmbId,6=BrkrOrCtdnPtcptId,
        // 7=ClctnDt,8=DueDt,9=ClntId,10=CtrySubDvsn,11=TckrSymb,12=SctySrs,13=FinInstrmId,
        // 14=FinInstrmTp,15=ISIN,16=XpryDt,17=StrkPric,18=OptnTp,
        // 19=TtlBuyTradgVol,20=TtlBuyTrfVal,21=TtlSellTradgVol,22=TtlSellTrfVal,
        // 23=BuyDlvryQty,24=BuyDlvryVal,25=BuyOthrThanDlvryQty,26=BuyOthrThanDlvryVal,
        // 27=BuyStmpDty,28=SellStmpDty,29=SttlmPric,30=BuyDlvryStmpDty,
        // 31=BuyOthrThanDlvryStmpDty,32=StmpDtyAmt
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

                var clntId = f[9].Trim();
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

                var xpryDt = f[16].Trim();
                var row = new FoStampDuty
                {
                    StampDutyRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    RptHdr = rptHdr,
                    TradDt = request.TradingDate,
                    Sgmt = f[1].Trim(),
                    Src = f[2].Trim(),
                    Exchange = request.Exchange,
                    ClrMmbId = f[5].Trim(),
                    TradngMmbId = f[6].Trim(),
                    ClntId = clntId,
                    ClientId = clientId,
                    ClientName = clientName,
                    ClientStateCode = clientStateCode,
                    CtrySubDvsn = f[10].Trim(),
                    TckrSymb = f[11].Trim(),
                    FinInstrmTp = f[14].Trim(),
                    Isin = f[15].Trim(),
                    XpryDt = xpryDt,
                    ExpiryDate = ImportFoTradeFileCommandHandler.ParseExpiryDate(xpryDt),
                    StrkPric = decimal.TryParse(f[17].Trim(), out var sp) ? sp : 0,
                    OptnTp = f[18].Trim(),
                    TtlBuyTradgVol = long.TryParse(f[19].Trim(), out var bvol) ? bvol : 0,
                    TtlBuyTrfVal = decimal.TryParse(f[20].Trim(), out var bval) ? bval : 0,
                    TtlSellTradgVol = long.TryParse(f[21].Trim(), out var svol) ? svol : 0,
                    TtlSellTrfVal = decimal.TryParse(f[22].Trim(), out var sval) ? sval : 0,
                    BuyDlvryQty = f.Length > 23 && long.TryParse(f[23].Trim(), out var bdq) ? bdq : 0,
                    BuyDlvryVal = f.Length > 24 && decimal.TryParse(f[24].Trim(), out var bdv) ? bdv : 0,
                    BuyOthrThanDlvryQty = f.Length > 25 && long.TryParse(f[25].Trim(), out var bodq) ? bodq : 0,
                    BuyOthrThanDlvryVal = f.Length > 26 && decimal.TryParse(f[26].Trim(), out var bodv) ? bodv : 0,
                    BuyStmpDty = f.Length > 27 && decimal.TryParse(f[27].Trim(), out var bsd) ? bsd : 0,
                    SellStmpDty = f.Length > 28 && decimal.TryParse(f[28].Trim(), out var ssd) ? ssd : 0,
                    SttlmPric = f.Length > 29 && decimal.TryParse(f[29].Trim(), out var sttl) ? sttl : 0,
                    BuyDlvryStmpDty = f.Length > 30 && decimal.TryParse(f[30].Trim(), out var bdsd) ? bdsd : 0,
                    BuyOthrThanDlvryStmpDty = f.Length > 31 && decimal.TryParse(f[31].Trim(), out var bodsd) ? bodsd : 0,
                    StmpDtyAmt = f.Length > 32 && decimal.TryParse(f[32].Trim(), out var sda) ? sda : 0
                };

                chunk.Add(row);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _stampRepo.AddRangeAsync(chunk, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _unitOfWork.ClearTracking();
                    chunk.Clear();
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
            await _stampRepo.AddRangeAsync(chunk, cancellationToken);
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
