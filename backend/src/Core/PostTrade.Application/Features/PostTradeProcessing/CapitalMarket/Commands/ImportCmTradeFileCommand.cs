using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record ImportCmTradeFileCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportCmTradeFileCommandHandler : IRequestHandler<ImportCmTradeFileCommand, ImportResultDto>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IRepository<CmTrade> _tradeRepo;
    private readonly IRepository<CmFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IRepository<CmSettlementMaster> _settlementMasterRepo;
    private readonly IRepository<CmScripMaster> _scripMasterRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportCmTradeFileCommandHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IRepository<CmTrade> tradeRepo,
        IRepository<CmFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IRepository<CmSettlementMaster> settlementMasterRepo,
        IRepository<CmScripMaster> scripMasterRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _tradeRepo = tradeRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _settlementMasterRepo = settlementMasterRepo;
        _scripMasterRepo = scripMasterRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportCmTradeFileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Prerequisite checks
        var hasSettlement = await _settlementMasterRepo.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.Exchange == request.Exchange && s.TradingDate == request.TradingDate,
            cancellationToken);
        if (hasSettlement == null)
            throw new PrerequisiteNotMetException(
                $"Settlement Master missing for Exchange '{request.Exchange}' on {request.TradingDate:yyyy-MM-dd}. Import the settlement file first.");

        var hasScrip = await _scripMasterRepo.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.Exchange == request.Exchange && s.TradingDate == request.TradingDate,
            cancellationToken);
        if (hasScrip == null)
            throw new PrerequisiteNotMetException(
                $"Scrip Master missing for Exchange '{request.Exchange}' on {request.TradingDate:yyyy-MM-dd}. Import the scrip master file first.");

        // Duplicate detection
        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == CmFileType.Trade &&
            b.Exchange == request.Exchange &&
            b.TradingDate == request.TradingDate &&
            b.Status == CmImportStatus.Completed,
            cancellationToken);

        if (existing != null)
            throw new DuplicateImportException(existing.BatchId);

        // Create batch record
        var batch = new CmFileImportBatch
        {
            BatchId = Guid.NewGuid(),
            TenantId = tenantId,
            FileType = CmFileType.Trade,
            Exchange = request.Exchange,
            TradingDate = request.TradingDate,
            Status = CmImportStatus.Processing,
            TriggerSource = Enum.TryParse<CmTriggerSource>(request.TriggerSource, out var src) ? src : CmTriggerSource.ManualUpload,
            FileName = request.FileName,
            StartedAt = DateTime.UtcNow
        };
        await _batchRepo.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load client map: ClientCode → ClientId
        var clients = await _clientRepo.GetAllAsync(cancellationToken);
        var clientMap = clients.ToDictionary(c => c.ClientCode, c => c.ClientId, StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<CmFileImportLog>();
        var chunk = new List<CmTrade>();
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
                if (fields.Length < 15)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new CmFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var clntId = fields[9].Trim();
                clientMap.TryGetValue(clntId, out var clientId);
                if (clientId == Guid.Empty && !string.IsNullOrEmpty(clntId))
                    logs.Add(new CmFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Client '{clntId}' not found in master" });

                var trade = new CmTrade
                {
                    TradeRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    UniqueTradeId = fields[12].Trim(),
                    TradDt = request.TradingDate,
                    Sgmt = fields[2].Trim(),
                    Src = fields[3].Trim(),
                    FinInstrmId = fields[4].Trim(),
                    FinInstrmNm = fields[5].Trim(),
                    TradngMmbId = fields[8].Trim(),
                    ClntId = clntId,
                    ClientId = clientId == Guid.Empty ? null : clientId,
                    OrdId = fields[11].Trim(),
                    BuySellInd = fields[13].Trim(),
                    TradQty = long.TryParse(fields[14].Trim(), out var qty) ? qty : 0,
                    PricePrUnit = fields.Length > 15 && decimal.TryParse(fields[15].Trim(), out var price) ? price : 0,
                    TradVal = fields.Length > 16 && decimal.TryParse(fields[16].Trim(), out var val) ? val : 0,
                    SttlmId = fields.Length > 17 ? fields[17].Trim() : string.Empty,
                    SttlmTyp = fields.Length > 18 ? fields[18].Trim() : string.Empty,
                    Exchange = request.Exchange
                };

                chunk.Add(trade);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _tradeRepo.AddRangeAsync(chunk, cancellationToken);
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
            await _tradeRepo.AddRangeAsync(chunk, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _unitOfWork.ClearTracking();
        }

        if (logs.Count > 0)
        {
            await _logRepo.AddRangeAsync(logs, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Reload and update batch status
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
