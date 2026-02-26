using MediatR;
using PostTrade.Application.Features.Trading.Positions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.Trading.Positions.Queries;

public record GetPositionsByClientQuery(Guid ClientId) : IRequest<IEnumerable<PositionDto>>;

public class GetPositionsByClientQueryHandler : IRequestHandler<GetPositionsByClientQuery, IEnumerable<PositionDto>>
{
    private readonly IRepository<Position> _repo;
    private readonly ITenantContext _tenantContext;

    public GetPositionsByClientQueryHandler(IRepository<Position> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<PositionDto>> Handle(GetPositionsByClientQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var positions = await _repo.FindAsync(
            p => p.TenantId == tenantId && p.ClientId == request.ClientId, cancellationToken);
        return positions.Select(GetPositionsQueryHandler.ToDto);
    }
}
