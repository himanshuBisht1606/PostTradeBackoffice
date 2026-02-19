using MediatR;
using PostTrade.Application.Features.Reconciliation.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Enums;
using ReconEntity = PostTrade.Domain.Entities.Reconciliation.Reconciliation;

namespace PostTrade.Application.Features.Reconciliation.Queries;

public record GetReconciliationsQuery(
    DateTime? ReconDate = null,
    ReconType? ReconType = null,
    ReconStatus? Status = null
) : IRequest<IEnumerable<ReconciliationDto>>;

public class GetReconciliationsQueryHandler : IRequestHandler<GetReconciliationsQuery, IEnumerable<ReconciliationDto>>
{
    private readonly IRepository<ReconEntity> _repo;
    private readonly ITenantContext _tenantContext;

    public GetReconciliationsQueryHandler(IRepository<ReconEntity> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ReconciliationDto>> Handle(GetReconciliationsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var records = await _repo.FindAsync(
            r => r.TenantId == tenantId &&
                 (request.ReconDate == null || r.ReconDate.Date == request.ReconDate.Value.Date) &&
                 (request.ReconType == null || r.ReconType == request.ReconType) &&
                 (request.Status == null || r.Status == request.Status),
            cancellationToken);

        return records
            .OrderByDescending(r => r.ReconDate)
            .Select(ToDto);
    }

    internal static ReconciliationDto ToDto(ReconEntity r) => new(
        r.ReconId, r.TenantId, r.ReconDate, r.SettlementNo,
        r.ReconType, r.SystemValue, r.ExchangeValue,
        r.Difference, r.ToleranceLimit, r.Status,
        r.Comments, r.ResolvedAt, r.ResolvedBy);
}
