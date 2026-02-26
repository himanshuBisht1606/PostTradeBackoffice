using MediatR;
using PostTrade.Application.Features.Trading.PnL.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.Trading.PnL.Queries;

public record GetPnLSnapshotByDateQuery(DateTime Date, Guid? ClientId = null) : IRequest<IEnumerable<PnLSnapshotDto>>;

public class GetPnLSnapshotByDateQueryHandler : IRequestHandler<GetPnLSnapshotByDateQuery, IEnumerable<PnLSnapshotDto>>
{
    private readonly IRepository<PnLSnapshot> _repo;
    private readonly ITenantContext _tenantContext;

    public GetPnLSnapshotByDateQueryHandler(IRepository<PnLSnapshot> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<PnLSnapshotDto>> Handle(GetPnLSnapshotByDateQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var date = request.Date.Date;

        var snapshots = await _repo.FindAsync(
            p => p.TenantId == tenantId &&
                 p.SnapshotDate.Date == date &&
                 (request.ClientId == null || p.ClientId == request.ClientId),
            cancellationToken);

        return snapshots.Select(GetPnLSnapshotsQueryHandler.ToDto);
    }
}
