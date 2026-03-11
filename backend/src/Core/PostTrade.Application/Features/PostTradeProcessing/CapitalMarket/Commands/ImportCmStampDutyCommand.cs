using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmStampDutyCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmStampDutyCommandHandler : IRequestHandler<ImportCmStampDutyCommand, ImportResultDto>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IRepository<CmStampDuty> _stampRepo;
    private readonly IRepository<CmFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportCmStampDutyCommandHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IRepository<CmStampDuty> stampRepo,
        IRepository<CmFileImportLog> logRepo,
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

    public async Task<ImportResultDto> Handle(ImportCmStampDutyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == CmFileType.StampDuty &&
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
            FileType = CmFileType.StampDuty,
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
        var clientMap = clients.ToDictionary(c => c.ClientCode, c => c.ClientId, StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<CmFileImportLog>();
        var chunk = new List<CmStampDuty>();
        int rowNum = 0, created = 0, skipped = 0;
        string currentRptHdr = string.Empty;

        using var reader = new StreamReader(request.FileStream);
        string? line;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("RptHdr", StringComparison.OrdinalIgnoreCase))
            {
                var hdrFields = CsvParser.ParseLine(line);
                currentRptHdr = hdrFields.Length > 1 ? hdrFields[1].Trim() : line.Trim();
                continue;
            }

            if (line.StartsWith("Rec Tp", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("TradngMmbId", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Record", StringComparison.OrdinalIgnoreCase))
                continue;

            rowNum++;
            try
            {
                var fields = CsvParser.ParseLine(line);
                if (fields.Length < 8)
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

                var row = new CmStampDuty
                {
                    StampDutyRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    RptHdr = currentRptHdr,
                    TradDt = request.TradingDate,
                    TradngMmbId = fields[0].Trim(),
                    ClntId = clntId,
                    ClientId = clientId == Guid.Empty ? null : clientId,
                    Sgmt = fields[3].Trim(),
                    IsinCode = fields[4].Trim(),
                    ScripNm = fields[5].Trim(),
                    BuySellInd = fields[6].Trim(),
                    TradQty = long.TryParse(fields[7].Trim(), out var qty) ? qty : 0,
                    TradVal = fields.Length > 8 && decimal.TryParse(fields[8].Trim(), out var tv) ? tv : 0,
                    StmpDtyAmt = fields.Length > 9 && decimal.TryParse(fields[9].Trim(), out var da) ? da : 0,
                    StmpDtyRate = fields.Length > 10 && decimal.TryParse(fields[10].Trim(), out var dr) ? dr : 0
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
                logs.Add(new CmFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = ex.Message, RawData = line[..Math.Min(line.Length, 1000)] });
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
