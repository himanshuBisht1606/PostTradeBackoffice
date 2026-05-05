using MediatR;
using PostTrade.Application.Features.MasterSetup.ClientSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.ClientSegments.Queries;

public record GetClientSegmentsQuery(Guid ClientId) : IRequest<IEnumerable<ClientSegmentActivationDto>>;

public class GetClientSegmentsQueryHandler : IRequestHandler<GetClientSegmentsQuery, IEnumerable<ClientSegmentActivationDto>>
{
    private readonly IRepository<ClientSegmentActivation> _repo;
    private readonly ITenantContext _tenantContext;

    public GetClientSegmentsQueryHandler(IRepository<ClientSegmentActivation> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ClientSegmentActivationDto>> Handle(GetClientSegmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(a => a.TenantId == tenantId && a.ClientId == request.ClientId, cancellationToken);
        return results.Select(a => new ClientSegmentActivationDto(a.ActivationId, a.TenantId,
            a.ClientId, a.ExchangeSegmentId, a.Status,
            a.ExposureLimit, a.MarginType, a.ActivatedOn, a.DeactivatedOn));
    }
}
