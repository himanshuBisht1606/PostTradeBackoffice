using MediatR;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.ExchangeSegments.Queries;

public record GetExchangeSegmentsQuery(Guid? ExchangeId = null, Guid? SegmentId = null) : IRequest<IEnumerable<ExchangeSegmentDto>>;

public class GetExchangeSegmentsQueryHandler : IRequestHandler<GetExchangeSegmentsQuery, IEnumerable<ExchangeSegmentDto>>
{
    private readonly IRepository<ExchangeSegment> _repo;
    private readonly ITenantContext _tenantContext;

    public GetExchangeSegmentsQueryHandler(IRepository<ExchangeSegment> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ExchangeSegmentDto>> Handle(GetExchangeSegmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(e =>
            e.TenantId == tenantId &&
            (request.ExchangeId == null || e.ExchangeId == request.ExchangeId) &&
            (request.SegmentId == null || e.SegmentId == request.SegmentId), cancellationToken);

        return results.Select(e => new ExchangeSegmentDto(e.ExchangeSegmentId, e.TenantId, e.ExchangeId,
            e.SegmentId, e.ExchangeSegmentCode, e.ExchangeSegmentName, e.SettlementType, e.IsActive));
    }
}
