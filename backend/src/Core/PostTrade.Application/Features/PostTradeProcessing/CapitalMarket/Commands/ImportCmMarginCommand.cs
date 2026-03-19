using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmMarginCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmMarginCommandHandler : IRequestHandler<ImportCmMarginCommand, ImportResultDto>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IRepository<CmMargin> _marginRepo;
    private readonly IRepository<CmFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportCmMarginCommandHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IRepository<CmMargin> marginRepo,
        IRepository<CmFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _marginRepo = marginRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportCmMarginCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == CmFileType.Margin &&
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
            FileType = CmFileType.Margin,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = CmImportStatus.Processing,
            TriggerSource = Enum.TryParse<CmTriggerSource>(request.TriggerSource, out var src) ? src : CmTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };
        await _batchRepo.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var clients = await _clientRepo.GetAllAsync(cancellationToken);
        var clientMap = clients.Where(c => !string.IsNullOrEmpty(c.ClientCode)).ToDictionary(c => c.ClientCode!, c => c.ClientId, StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<CmFileImportLog>();
        var chunk = new List<CmMargin>();
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

                var clntId = fields[2].Trim();
                clientMap.TryGetValue(clntId, out var clientId);
                if (clientId == Guid.Empty && !string.IsNullOrEmpty(clntId))
                    logs.Add(new CmFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Client '{clntId}' not found in master" });

                var row = new CmMargin
                {
                    MarginRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradDt = request.TradingDate,
                    TradngMmbId = fields[0].Trim(),
                    ClntId = clntId,
                    ClientId = clientId == Guid.Empty ? null : clientId,
                    Sgmt = fields[3].Trim(),
                    IsinCode = fields[4].Trim(),
                    ScripNm = fields[5].Trim(),
                    MtmMrgnAmt = decimal.TryParse(fields[6].Trim(), out var m1) ? m1 : 0,
                    VrMrgnAmt = decimal.TryParse(fields[7].Trim(), out var m2) ? m2 : 0,
                    ExpsrMrgnAmt = decimal.TryParse(fields[8].Trim(), out var m3) ? m3 : 0,
                    AddhcMrgnAmt = decimal.TryParse(fields[9].Trim(), out var m4) ? m4 : 0,
                    CrystldLssAmt = fields.Length > 10 && decimal.TryParse(fields[10].Trim(), out var m5) ? m5 : 0,
                    TtlMrgnAmt = fields.Length > 11 && decimal.TryParse(fields[11].Trim(), out var m6) ? m6 : 0
                };

                chunk.Add(row);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _marginRepo.AddRangeAsync(chunk, cancellationToken);
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
            await _marginRepo.AddRangeAsync(chunk, cancellationToken);
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
