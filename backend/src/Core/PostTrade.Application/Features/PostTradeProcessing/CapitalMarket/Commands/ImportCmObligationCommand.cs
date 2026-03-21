using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmObligationCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmObligationCommandHandler : IRequestHandler<ImportCmObligationCommand, ImportResultDto>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IRepository<CmObligation> _obligationRepo;
    private readonly IRepository<CmFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportCmObligationCommandHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IRepository<CmObligation> obligationRepo,
        IRepository<CmFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _obligationRepo = obligationRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportCmObligationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == CmFileType.Obligation &&
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
            FileType = CmFileType.Obligation,
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
        var chunk = new List<CmObligation>();
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

                DateOnly.TryParse(fields[5].Trim(), out var sttlmDt);

                var row = new CmObligation
                {
                    ObligationRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradDt = request.TradingDate,
                    TradngMmbId = fields[0].Trim(),
                    ClntId = clntId,
                    ClientId = clientId == Guid.Empty ? null : clientId,
                    SttlmId = fields[3].Trim(),
                    SttlmDt = sttlmDt,
                    IsinCode = fields[6].Trim(),
                    ScripNm = fields[7].Trim(),
                    ObligTyp = fields[8].Trim(),
                    NetQty = long.TryParse(fields[9].Trim(), out var qty) ? qty : 0,
                    ObligStdAmt = fields.Length > 10 && decimal.TryParse(fields[10].Trim(), out var a1) ? a1 : 0,
                    CrObligStdAmt = fields.Length > 11 && decimal.TryParse(fields[11].Trim(), out var a2) ? a2 : 0,
                    DrObligStdAmt = fields.Length > 12 && decimal.TryParse(fields[12].Trim(), out var a3) ? a3 : 0
                };

                chunk.Add(row);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _obligationRepo.AddRangeAsync(chunk, cancellationToken);
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
            await _obligationRepo.AddRangeAsync(chunk, cancellationToken);
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
