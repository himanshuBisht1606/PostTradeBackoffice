using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Exceptions;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record ImportFoPositionCommand(
    Stream FileStream,
    DateOnly TradingDate,
    string Exchange,        // NFO (NCL clears both)
    string TriggerSource,
    string FileName
) : IRequest<ImportResultDto>;

public class ImportFoPositionCommandHandler : IRequestHandler<ImportFoPositionCommand, ImportResultDto>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IRepository<FoPosition> _positionRepo;
    private readonly IRepository<FoFileImportLog> _logRepo;
    private readonly IRepository<Client> _clientRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private const int BatchSize = 2000;

    public ImportFoPositionCommandHandler(
        IRepository<FoFileImportBatch> batchRepo,
        IRepository<FoPosition> positionRepo,
        IRepository<FoFileImportLog> logRepo,
        IRepository<Client> clientRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _positionRepo = positionRepo;
        _logRepo = logRepo;
        _clientRepo = clientRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoPositionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = await _batchRepo.FirstOrDefaultAsync(b =>
            b.TenantId == tenantId &&
            b.FileType == FoFileType.Position &&
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
            FileType = FoFileType.Position,
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
        var chunk = new List<FoPosition>();
        int rowNum = 0, created = 0, skipped = 0;

        // File columns:
        // 0=Sgmt,1=Src,2=RptgDt,3=BizDt,4=TradRegnOrgn,5=ClrMmbId,6=BrkrOrCtdnPtcptId,
        // 7=ClntTp,8=ClntId,9=FinInstrmTp,10=ISIN,11=TckrSymb,12=XpryDt,
        // 13=FininstrmActlXpryDt,14=StrkPric,15=OptnTp,16=NewBrdLotQty,
        // 17=OpngLngQty,18=OpngLngVal,19=OpngShrtQty,20=OpngShrtVal,
        // 21=OpnBuyTradgQty,22=OpnBuyTradgVal,23=OpnSellTradgQty,24=OpnSellTradgVal,
        // 25=PreExrcAssgndLngQty,26=PreExrcAssgndLngVal,27=PreExrcAssgndShrtQty,28=PreExrcAssgndShrtVal,
        // 29=ExrcdQty,30=AssgndQty,31=PstExrcAssgndLngQty,32=PstExrcAssgndLngVal,
        // 33=PstExrcAssgndShrtQty,34=PstExrcAssgndShrtVal,
        // 35=SttlmPric,36=RefRate,37=PrmAmt,38=DalyMrkToMktSettlmVal,39=FutrsFnlSttlmVal,40=ExrcAssgndVal
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

                var clntId = f[8].Trim();
                if (string.IsNullOrEmpty(clntId))
                {
                    skipped++;
                    continue;
                }

                clientMap.TryGetValue(clntId, out var clientId);
                if (clientId == Guid.Empty)
                    logs.Add(new FoFileImportLog { LogId = Guid.NewGuid(), BatchId = batch.BatchId, RowNumber = rowNum, Level = "Warning", Message = $"Client '{clntId}' not found in master" });

                var pos = new FoPosition
                {
                    PositionRowId = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    TenantId = tenantId,
                    TradDt = request.TradingDate,
                    Sgmt = f[0].Trim(),
                    Src = f[1].Trim(),
                    Exchange = request.Exchange,
                    ClrMmbId = f[5].Trim(),
                    TradngMmbId = f[6].Trim(),
                    ClntTp = f[7].Trim(),
                    ClntId = clntId,
                    ClientId = clientId == Guid.Empty ? null : clientId,
                    FinInstrmTp = f[9].Trim(),
                    Isin = f[10].Trim(),
                    TckrSymb = f[11].Trim(),
                    XpryDt = f[12].Trim(),
                    StrkPric = decimal.TryParse(f[14].Trim(), out var sp) ? sp : 0,
                    OptnTp = f[15].Trim(),
                    NewBrdLotQty = long.TryParse(f[16].Trim(), out var lot) ? lot : 0,
                    OpngLngQty = long.TryParse(f[17].Trim(), out var olq) ? olq : 0,
                    OpngLngVal = decimal.TryParse(f[18].Trim(), out var olv) ? olv : 0,
                    OpngShrtQty = long.TryParse(f[19].Trim(), out var osq) ? osq : 0,
                    OpngShrtVal = decimal.TryParse(f[20].Trim(), out var osv) ? osv : 0,
                    OpnBuyTradgQty = long.TryParse(f[21].Trim(), out var obtq) ? obtq : 0,
                    OpnBuyTradgVal = decimal.TryParse(f[22].Trim(), out var obtv) ? obtv : 0,
                    OpnSellTradgQty = long.TryParse(f[23].Trim(), out var ostq) ? ostq : 0,
                    OpnSellTradgVal = decimal.TryParse(f[24].Trim(), out var ostv) ? ostv : 0,
                    PreExrcAssgndLngQty = f.Length > 25 && long.TryParse(f[25].Trim(), out var pelq) ? pelq : 0,
                    PreExrcAssgndLngVal = f.Length > 26 && decimal.TryParse(f[26].Trim(), out var pelv) ? pelv : 0,
                    PreExrcAssgndShrtQty = f.Length > 27 && long.TryParse(f[27].Trim(), out var pesq) ? pesq : 0,
                    PreExrcAssgndShrtVal = f.Length > 28 && decimal.TryParse(f[28].Trim(), out var pesv) ? pesv : 0,
                    ExrcdQty = f.Length > 29 && long.TryParse(f[29].Trim(), out var eq) ? eq : 0,
                    AssgndQty = f.Length > 30 && long.TryParse(f[30].Trim(), out var aq) ? aq : 0,
                    PstExrcAssgndLngQty = f.Length > 31 && long.TryParse(f[31].Trim(), out var poelq) ? poelq : 0,
                    PstExrcAssgndLngVal = f.Length > 32 && decimal.TryParse(f[32].Trim(), out var poelv) ? poelv : 0,
                    PstExrcAssgndShrtQty = f.Length > 33 && long.TryParse(f[33].Trim(), out var poesq) ? poesq : 0,
                    PstExrcAssgndShrtVal = f.Length > 34 && decimal.TryParse(f[34].Trim(), out var poesv) ? poesv : 0,
                    SttlmPric = f.Length > 35 && decimal.TryParse(f[35].Trim(), out var sttl) ? sttl : 0,
                    RefRate = f.Length > 36 && decimal.TryParse(f[36].Trim(), out var rr) ? rr : 0,
                    PrmAmt = f.Length > 37 && decimal.TryParse(f[37].Trim(), out var pa) ? pa : 0,
                    DalyMrkToMktSettlmVal = f.Length > 38 && decimal.TryParse(f[38].Trim(), out var dmtm) ? dmtm : 0,
                    FutrsFnlSttlmVal = f.Length > 39 && decimal.TryParse(f[39].Trim(), out var ffs) ? ffs : 0,
                    ExrcAssgndVal = f.Length > 40 && decimal.TryParse(f[40].Trim(), out var eav) ? eav : 0
                };

                chunk.Add(pos);
                created++;

                if (chunk.Count >= BatchSize)
                {
                    await _positionRepo.AddRangeAsync(chunk, cancellationToken);
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
            await _positionRepo.AddRangeAsync(chunk, cancellationToken);
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
