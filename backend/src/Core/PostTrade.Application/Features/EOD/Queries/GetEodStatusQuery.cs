using MediatR;
using PostTrade.Application.Features.EOD.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.EOD.Queries;

public record GetEodStatusQuery(DateTime Date) : IRequest<EodStatusDto>;

public class GetEodStatusQueryHandler : IRequestHandler<GetEodStatusQuery, EodStatusDto>
{
    private readonly IRepository<PnLSnapshot> _snapshotRepo;
    private readonly ITenantContext _tenantContext;

    public GetEodStatusQueryHandler(IRepository<PnLSnapshot> snapshotRepo, ITenantContext tenantContext)
    {
        _snapshotRepo = snapshotRepo;
        _tenantContext = tenantContext;
    }

    public async Task<EodStatusDto> Handle(GetEodStatusQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var date = request.Date.Date;
        var nextDay = date.AddDays(1);

        var snapshots = await _snapshotRepo.FindAsync(
            s => s.TenantId == tenantId && s.SnapshotDate >= date && s.SnapshotDate < nextDay,
            cancellationToken);

        var snapshotList = snapshots.ToList();
        var isProcessed = snapshotList.Count > 0;
        var processedAt = isProcessed
            ? snapshotList.Max(s => s.SnapshotTime)
            : (DateTime?)null;

        return new EodStatusDto(date, isProcessed, snapshotList.Count, processedAt);
    }
}
