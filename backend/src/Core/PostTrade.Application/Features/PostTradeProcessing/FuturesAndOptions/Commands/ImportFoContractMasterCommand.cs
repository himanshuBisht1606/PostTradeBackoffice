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
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoContractMasterCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoContractMaster> contractRepo,
        IRepository<FoFileImportLog> logRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _contractRepo = contractRepo;
        _logRepo = logRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoContractMasterCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // For contract master, delete existing records for this exchange/date and reimport
        var existingBatch = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.ContractMaster &&
            b.Exchange == request.Exchange &&
            b.TradingDate == request.TradingDate &&
            b.Status == FoImportStatus.Completed,
            cancellationToken);

        if (existingBatch != null)
        {
            // Delete old contract master rows for re-import
            await _unitOfWork.ExecuteDeleteAsync<FoContractMaster>(
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
        var chunk = new List<FoContractMaster>();
        int rowNum = 0, created = 0, skipped = 0;

        // Key columns (both NSE and BSE share the same header layout):
        // 0=FinInstrmId,1=UndrlygFinInstrmId,2=FinInstrmNm(type code),3=TckrSymb,4=XpryDt,
        // 5=StrkPric,6=OptnTp,8=MinLot,9=NewBrdLotQty,10=BidIntrvl,
        // 18=StockNm(NSE: full contract name; BSE: sector code),
        // 19=SttlmMtd(NSE: C/D; BSE: full contract name),
        // 20=BasePric(NSE: base price; BSE: delivery flag shifted),
        // 27=MktTpAndId,60=OptnExrcStyle,110=ISIN
        // FinInstrmTp is derived from f[2] via MapInstrumentType (NOT f[62] which is ExrcRjctAllwd)
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

                // f[2] holds the exchange-specific type code (e.g. OPTIDX, FUTSTK for NSE; SO, IO, SF for BSE)
                var finInstrmTp = ImportFoTradeFileCommandHandler.MapInstrumentType(f[2].Trim());

                // Full contract name: NSE stores at f[18] (StockNm column),
                // BSE shifts it to f[19] (SttlmMtd column)
                var fullContractName = isNse
                    ? (f.Length > 18 ? f[18].Trim() : string.Empty)
                    : (f.Length > 19 ? f[19].Trim() : string.Empty);

                // Settlement method: NSE f[19] = C/D; BSE column is occupied by contract name
                var sttlmMtd = isNse && f.Length > 19 ? f[19].Trim() : string.Empty;

                // Base price only reliable for NSE (BSE shifts column values in 20-21 range)
                decimal? basePric = null;
                if (isNse && f.Length > 20 &&
                    decimal.TryParse(f[20].Trim(), out var bp) && bp > 0)
                    basePric = bp;

                var contract = new FoContractMaster
                {
                    ContractRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradingDate = request.TradingDate,
                    Exchange = request.Exchange,
                    FinInstrmId = f[0].Trim(),
                    UndrlygFinInstrmId = f[1].Trim(),
                    FinInstrmNm = fullContractName,
                    TckrSymb = f[3].Trim(),
                    XpryDt = xpryDt,
                    ExpiryDate = ImportFoTradeFileCommandHandler.ParseExpiryDate(xpryDt),
                    StrkPric = decimal.TryParse(f[5].Trim(), out var sp) ? sp : 0,
                    OptnTp = f[6].Trim(),
                    MinLot = long.TryParse(f[8].Trim(), out var ml) ? ml : 0,
                    NewBrdLotQty = long.TryParse(f[9].Trim(), out var lot) ? lot : 0,
                    TickSize = f.Length > 10 && decimal.TryParse(f[10].Trim(), out var ts) ? ts : 0,
                    StockNm = isNse ? fullContractName : (f.Length > 18 ? f[18].Trim() : string.Empty),
                    SttlmMtd = sttlmMtd,
                    BasePric = basePric,
                    MktTpAndId = f.Length > 27 ? NullIfBlank(f[27]) : null,
                    OptnExrcStyle = f.Length > 60 ? NullIfBlank(f[60]) : null,
                    Isin = f.Length > 110 ? NullIfBlank(f[110]) : null,
                    FinInstrmTp = finInstrmTp
                };

                chunk.Add(contract);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _contractRepo.AddRangeAsync(chunk, cancellationToken);
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
            await _contractRepo.AddRangeAsync(chunk, cancellationToken);
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

    private static string? NullIfBlank(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
