using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Queries;

public record GetCmImportBatchLogsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 50
) : IRequest<CmImportBatchLogsPagedDto>;

public class GetCmImportBatchLogsQueryHandler : IRequestHandler<GetCmImportBatchLogsQuery, CmImportBatchLogsPagedDto>
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

    public async Task<CmImportBatchLogsPagedDto> Handle(GetCmImportBatchLogsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Verify batch belongs to this tenant
        var batch = await _batchRepo.FirstOrDefaultAsync(
            b => b.BatchId == request.BatchId && b.TenantId == tenantId,
            cancellationToken);

        if (batch == null) return new CmImportBatchLogsPagedDto([], [], 0, request.Page, request.PageSize);

        var all = await _logRepo.FindAsync(l => l.BatchId == request.BatchId, cancellationToken);
        var ordered = all.OrderBy(l => l.RowNumber).ToList();

        // Distinct summary — group by (Level, Message), count occurrences
        var summary = ordered
            .GroupBy(l => new { l.Level, l.Message })
            .Select(g => new CmImportBatchLogSummaryItemDto(g.Key.Level, g.Key.Message, g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();

        // Paginated individual rows
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var page = Math.Max(1, request.Page);
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new CmImportBatchLogDto(l.LogId, l.BatchId, l.RowNumber, l.Level, l.Message, l.RawData))
            .ToList();

        return new CmImportBatchLogsPagedDto(summary, items, totalCount, page, pageSize);
    }
}
