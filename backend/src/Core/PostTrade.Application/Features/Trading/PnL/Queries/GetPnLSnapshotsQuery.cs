using MediatR;
using PostTrade.Application.Features.Trading.PnL.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.Trading.PnL.Queries;

public record GetPnLSnapshotsQuery(Guid? ClientId = null) : IRequest<IEnumerable<PnLSnapshotDto>>;

public class GetPnLSnapshotsQueryHandler : IRequestHandler<GetPnLSnapshotsQuery, IEnumerable<PnLSnapshotDto>>
{
    private readonly IRepository<PnLSnapshot> _repo;
    private readonly ITenantContext _tenantContext;

    public GetPnLSnapshotsQueryHandler(IRepository<PnLSnapshot> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<PnLSnapshotDto>> Handle(GetPnLSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var snapshots = await _repo.FindAsync(
            p => p.TenantId == tenantId &&
                 (request.ClientId == null || p.ClientId == request.ClientId),
            cancellationToken);

        return snapshots
            .OrderByDescending(p => p.SnapshotDate)
            .Select(ToDto);
    }

    internal static PnLSnapshotDto ToDto(PnLSnapshot p) => new(
        p.PnLId, p.TenantId, p.ClientId, p.InstrumentId,
        p.SnapshotDate, p.SnapshotTime, p.RealizedPnL, p.UnrealizedPnL,
        p.TotalPnL, p.Brokerage, p.Taxes, p.NetPnL,
        p.OpenQuantity, p.AveragePrice, p.MarketPrice);
}
