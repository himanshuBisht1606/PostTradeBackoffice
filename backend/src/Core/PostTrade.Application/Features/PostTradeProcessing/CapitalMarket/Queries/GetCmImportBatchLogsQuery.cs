using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Queries;

public record GetCmImportBatchLogsQuery(Guid BatchId) : IRequest<IEnumerable<CmImportBatchLogDto>>;

public class GetCmImportBatchLogsQueryHandler : IRequestHandler<GetCmImportBatchLogsQuery, IEnumerable<CmImportBatchLogDto>>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IRepository<CmFileImportLog> _logRepo;
    private readonly ITenantContext _tenantContext;

    public GetCmImportBatchLogsQueryHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IRepository<CmFileImportLog> logRepo,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _logRepo = logRepo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CmImportBatchLogDto>> Handle(GetCmImportBatchLogsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Verify batch belongs to this tenant (query filter applies TenantId)
        var batch = await _batchRepo.FirstOrDefaultAsync(
            b => b.BatchId == request.BatchId && b.TenantId == tenantId,
            cancellationToken);

        if (batch == null) return [];

        var logs = await _logRepo.FindAsync(l => l.BatchId == request.BatchId, cancellationToken);

        return logs
            .OrderBy(l => l.RowNumber)
            .Select(l => new CmImportBatchLogDto(l.LogId, l.BatchId, l.RowNumber, l.Level, l.Message, l.RawData))
            .ToList();
    }
}
