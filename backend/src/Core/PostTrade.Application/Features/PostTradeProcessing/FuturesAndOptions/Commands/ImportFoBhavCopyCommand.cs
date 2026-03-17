using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record ImportFoBhavCopyCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportFoBhavCopyCommandHandler : IRequestHandler<ImportFoBhavCopyCommand, ImportResultDto>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IRepository<FoBhavCopy> _bhavRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoBhavCopyCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoBhavCopy> bhavRepo,
        IRepository<FoFileImportLog> logRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _bhavRepo = bhavRepo;
        _logRepo = logRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoBhavCopyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.BhavCopy &&
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
            FileType = FoFileType.BhavCopy,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = FoImportStatus.Processing,
            TriggerSource = Enum.TryParse<FoTriggerSource>(request.TriggerSource, out var src) ? src : FoTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };
        await _batchRepo.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        var chunk = new List<FoBhavCopy>();
        int rowNum = 0, created = 0, skipped = 0;

        // File columns:
        // 0=TradDt,1=BizDt,2=Sgmt,3=Src,4=FinInstrmTp,5=FinInstrmId,6=ISIN,7=TckrSymb,
        // 8=SctySrs,9=XpryDt,10=FininstrmActlXpryDt,11=StrkPric,12=OptnTp,13=FinInstrmNm,
        // 14=OpnPric,15=HghPric,16=LwPric,17=ClsPric,18=LastPric,19=PrvsClsgPric,
        // 20=UndrlygPric,21=SttlmPric,22=OpnIntrst,23=ChngInOpnIntrst,
        // 24=TtlTradgVol,25=TtlTrfVal,26=TtlNbOfTxsExctd,27=SsnId,28=NewBrdLotQty
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
                if (f.Length < 25)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var row = new FoBhavCopy
                {
                    BhavCopyRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradDt = request.TradingDate,
                    Exchange = request.Exchange,
                    Sgmt = f[2].Trim(),
                    Src = f[3].Trim(),
                    FinInstrmTp = f[4].Trim(),
                    FinInstrmId = f[5].Trim(),
                    Isin = f[6].Trim(),
                    TckrSymb = f[7].Trim(),
                    SctySrs = f[8].Trim(),
                    XpryDt = f[9].Trim(),
                    StrkPric = decimal.TryParse(f[11].Trim(), out var sp) ? sp : 0,
                    OptnTp = f[12].Trim(),
                    FinInstrmNm = f[13].Trim(),
                    OpnPric = decimal.TryParse(f[14].Trim(), out var op) ? op : 0,
                    HghPric = decimal.TryParse(f[15].Trim(), out var hp) ? hp : 0,
                    LwPric = decimal.TryParse(f[16].Trim(), out var lp) ? lp : 0,
                    ClsPric = decimal.TryParse(f[17].Trim(), out var cp) ? cp : 0,
                    LastPric = decimal.TryParse(f[18].Trim(), out var las) ? las : 0,
                    PrvsClsgPric = decimal.TryParse(f[19].Trim(), out var prev) ? prev : 0,
                    UndrlygPric = decimal.TryParse(f[20].Trim(), out var und) ? und : 0,
                    SttlmPric = decimal.TryParse(f[21].Trim(), out var sttl) ? sttl : 0,
                    OpnIntrst = long.TryParse(f[22].Trim(), out var oi) ? oi : 0,
                    ChngInOpnIntrst = long.TryParse(f[23].Trim(), out var coi) ? coi : 0,
                    TtlTradgVol = long.TryParse(f[24].Trim(), out var vol) ? vol : 0,
                    TtlTrfVal = decimal.TryParse(f[25].Trim(), out var tv) ? tv : 0,
                    TtlNbOfTxsExctd = f.Length > 26 && long.TryParse(f[26].Trim(), out var nb) ? nb : 0,
                    NewBrdLotQty = f.Length > 28 && long.TryParse(f[28].Trim(), out var lot) ? lot : 0
                };

                chunk.Add(row);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _bhavRepo.AddRangeAsync(chunk, cancellationToken);
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
            await _bhavRepo.AddRangeAsync(chunk, cancellationToken);
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
