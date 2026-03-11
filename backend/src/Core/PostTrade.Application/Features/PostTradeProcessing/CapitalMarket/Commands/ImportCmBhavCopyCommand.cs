using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmBhavCopyCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmBhavCopyCommandHandler : IRequestHandler<ImportCmBhavCopyCommand, ImportResultDto>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IRepository<CmBhavCopy> _bhavRepo;
    private readonly IRepository<CmFileImportLog> _logRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportCmBhavCopyCommandHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IRepository<CmBhavCopy> bhavRepo,
        IRepository<CmFileImportLog> logRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _bhavRepo = bhavRepo;
        _logRepo = logRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportCmBhavCopyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == CmFileType.BhavCopy &&
            b.Exchange == request.Exchange &&
            b.TradingDate == request.TradingDate &&
            b.Status == CmImportStatus.Completed,
            cancellationToken);

        if (existing != null)
            throw new DuplicateImportException(existing.BatchId);

        var batch = new CmFileImportBatch
        {
            BatchId = Guid.NewGuid(),
            TenantId = tenantId,
            FileType = CmFileType.BhavCopy,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = CmImportStatus.Processing,
            TriggerSource = Enum.TryParse<CmTriggerSource>(request.TriggerSource, out var src) ? src : CmTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };
        await _batchRepo.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var errors = new List<ImportErrorDto>();
        var logs = new List<CmFileImportLog>();
        var chunk = new List<CmBhavCopy>();
        int rowNum = 0, created = 0, skipped = 0;

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
                var fields = CsvParser.ParseLine(line);
                if (fields.Length < 10)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new CmFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var row = new CmBhavCopy
                {
                    BhavCopyRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradDt = request.TradingDate,
                    FinInstrmId = fields[0].Trim(),
                    FinInstrmNm = fields[1].Trim(),
                    Isin = fields[2].Trim(),
                    SctySrs = fields[3].Trim(),
                    OpnPric = decimal.TryParse(fields[4].Trim(), out var o) ? o : 0,
                    HghPric = decimal.TryParse(fields[5].Trim(), out var h) ? h : 0,
                    LwPric = decimal.TryParse(fields[6].Trim(), out var l) ? l : 0,
                    ClsPric = decimal.TryParse(fields[7].Trim(), out var c) ? c : 0,
                    LastPric = decimal.TryParse(fields[8].Trim(), out var lp) ? lp : 0,
                    PrvClsgPric = decimal.TryParse(fields[9].Trim(), out var pc) ? pc : 0,
                    TtlTradgVol = fields.Length > 10 && long.TryParse(fields[10].Trim(), out var vol) ? vol : 0,
                    TtlTrfVal = fields.Length > 11 && decimal.TryParse(fields[11].Trim(), out var tv) ? tv : 0,
                    MktCpzn = fields.Length > 12 && decimal.TryParse(fields[12].Trim(), out var mc) ? mc : null,
                    Exchange = request.Exchange
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
                logs.Add(new CmFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = ex.Message, RawData = line[..Math.Min(line.Length, 1000)] });
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
            batchToUpdate.Status = CmImportStatus.Completed;
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
