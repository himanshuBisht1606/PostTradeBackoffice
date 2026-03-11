using MediatR;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.ExchangeSegments.Queries;

public record GetExchangeSegmentByIdQuery(Guid ExchangeSegmentId) : IRequest<ExchangeSegmentDto?>;

public class GetExchangeSegmentByIdQueryHandler : IRequestHandler<GetExchangeSegmentByIdQuery, ExchangeSegmentDto?>
{
    private readonly IRepository<ExchangeSegment> _repo;
    private readonly ITenantContext _tenantContext;

    public GetExchangeSegmentByIdQueryHandler(IRepository<ExchangeSegment> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<ExchangeSegmentDto?> Handle(GetExchangeSegmentByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(e => e.ExchangeSegmentId == request.ExchangeSegmentId && e.TenantId == tenantId, cancellationToken);
        var e = results.FirstOrDefault();
        if (e is null) return null;
        return new ExchangeSegmentDto(e.ExchangeSegmentId, e.TenantId, e.ExchangeId,
            e.SegmentId, e.ExchangeSegmentCode, e.ExchangeSegmentName, e.SettlementType, e.IsActive);
    }
}
