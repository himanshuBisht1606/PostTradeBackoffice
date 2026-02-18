using MediatR;
using PostTrade.Application.Features.MasterSetup.Segments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Segments.Queries;

public record GetSegmentsQuery(Guid? ExchangeId = null) : IRequest<IEnumerable<SegmentDto>>;

public class GetSegmentsQueryHandler : IRequestHandler<GetSegmentsQuery, IEnumerable<SegmentDto>>
{
    private readonly IRepository<Segment> _repo;
    private readonly ITenantContext _tenantContext;

    public GetSegmentsQueryHandler(IRepository<Segment> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<SegmentDto>> Handle(GetSegmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var segments = await _repo.FindAsync(s =>
            s.TenantId == tenantId &&
            (request.ExchangeId == null || s.ExchangeId == request.ExchangeId), cancellationToken);
        return segments.Select(s => new SegmentDto(s.SegmentId, s.TenantId, s.ExchangeId, s.SegmentCode, s.SegmentName, s.IsActive));
    }
}
