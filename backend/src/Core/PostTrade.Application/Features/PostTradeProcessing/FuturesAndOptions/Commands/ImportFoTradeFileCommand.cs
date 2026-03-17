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
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IRepository<FoContractMaster> _contractMasterRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoTradeFileCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoTrade> tradeRepo,
        IRepository<FoFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IRepository<FoContractMaster> contractMasterRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _tradeRepo = tradeRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _contractMasterRepo = contractMasterRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoTradeFileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Prerequisite: contract master must exist for this exchange and trading date
        var hasContract = await _contractMasterRepo.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Exchange == request.Exchange && c.TradingDate == request.TradingDate,
            cancellationToken);
        if (hasContract == null)
            throw new PrerequisiteNotMetException(
                $"FO Contract Master missing for Exchange '{request.Exchange}' on {request.TradingDate:yyyy-MM-dd}. Import the contract master file first.");

        // Duplicate detection
        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.Trade &&
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
            FileType = FoFileType.Trade,
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
        var clientMap = clients.ToDictionary(c => c.ClientCode, c => c.ClientId, StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportErrorDto>();
        var logs = new List<FoFileImportLog>();
        var chunk = new List<FoTrade>();
        int rowNum = 0, created = 0, skipped = 0;

        // File columns:
        // 0=TradDt,1=BizDt,2=Sgmt,3=Src,4=Xchg,5=ClrMmbId,6=Brkr,7=FinInstrmTp,8=FinInstrmId,
        // 9=ISIN,10=TckrSymb,11=SctySrs,12=XpryDt,13=FininstrmActlXpryDt,14=StrkPric,15=OptnTp,
        // 16=FinInstrmNm,17=ClntTp,18=ClntId,...,22=SttlmTp,23=SctiesSttlmTxId,
        // 24=BuySellInd,25=TradQty,26=NewBrdLotQty,27=Pric,28=UnqTradIdr
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
                if (f.Length < 29)
                {
                    errors.Add(new ImportErrorDto(rowNum, "Insufficient columns"));
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = "Insufficient columns", RawData = line[..Math.Min(line.Length, 1000)] });
                    skipped++;
                    continue;
                }

                var clntId = f[18].Trim();
                clientMap.TryGetValue(clntId, out var clientId);
                if (clientId == Guid.Empty && !string.IsNullOrEmpty(clntId))
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Client '{clntId}' not found in master" });

                var trade = new FoTrade
                {
                    TradeRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    UniqueTradeId = f[28].Trim(),
                    TradDt = request.TradingDate,
                    Sgmt = f[2].Trim(),
                    Src = f[3].Trim(),
                    Exchange = request.Exchange,
                    TradngMmbId = f[6].Trim(),
                    FinInstrmTp = f[7].Trim(),
                    FinInstrmId = f[8].Trim(),
                    Isin = f[9].Trim(),
                    TckrSymb = f[10].Trim(),
                    XpryDt = f[12].Trim(),
                    StrkPric = decimal.TryParse(f[14].Trim(), out var sp) ? sp : 0,
                    OptnTp = f[15].Trim(),
                    FinInstrmNm = f[16].Trim(),
                    ClntTp = f[17].Trim(),
                    ClntId = clntId,
                    ClientId = clientId == Guid.Empty ? null : clientId,
                    SttlmTp = f[22].Trim(),
                    SctiesSttlmTxId = f[23].Trim(),
                    BuySellInd = f[24].Trim(),
                    TradQty = long.TryParse(f[25].Trim(), out var qty) ? qty : 0,
                    NewBrdLotQty = long.TryParse(f[26].Trim(), out var lot) ? lot : 0,
                    Pric = decimal.TryParse(f[27].Trim(), out var pric) ? pric : 0
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
                logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Error", Message = ex.Message, RawData = line[..Math.Min(line.Length, 1000)] });
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
