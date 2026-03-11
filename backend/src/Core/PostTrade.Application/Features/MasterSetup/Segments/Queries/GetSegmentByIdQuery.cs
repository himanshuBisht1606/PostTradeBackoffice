using MediatR;
using PostTrade.Application.Features.MasterSetup.Segments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Segments.Queries;

public record GetSegmentByIdQuery(Guid SegmentId) : IRequest<SegmentDto?>;

public class GetSegmentByIdQueryHandler : IRequestHandler<GetSegmentByIdQuery, SegmentDto?>
{
    private readonly IRepository<Segment> _repo;
    private readonly ITenantContext _tenantContext;

    public GetSegmentByIdQueryHandler(IRepository<Segment> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<SegmentDto?> Handle(GetSegmentByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(s => s.SegmentId == request.SegmentId && s.TenantId == tenantId, cancellationToken);
        var s = results.FirstOrDefault();
        if (s is null) return null;
        return new SegmentDto(s.SegmentId, s.TenantId, s.SegmentCode, s.SegmentName, s.Description, s.IsActive);
    }
}
