using MediatR;
using PostTrade.Application.Features.Reconciliation.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;
using ReconEntity = PostTrade.Domain.Entities.Reconciliation.Reconciliation;

namespace PostTrade.Application.Features.Reconciliation.Queries;

public record GetReconStatsQuery : IRequest<ReconStatsDto>;

public class GetReconStatsQueryHandler : IRequestHandler<GetReconStatsQuery, ReconStatsDto>
{
    private readonly IRepository<ReconEntity> _reconRepo;
    private readonly IRepository<ReconException> _exceptionRepo;
    private readonly ITenantContext _tenantContext;

    public GetReconStatsQueryHandler(
        IRepository<ReconEntity> reconRepo,
        IRepository<ReconException> exceptionRepo,
        ITenantContext tenantContext)
    {
        _reconRepo = reconRepo;
        _exceptionRepo = exceptionRepo;
        _tenantContext = tenantContext;
    }

    public async Task<ReconStatsDto> Handle(GetReconStatsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var today = DateTime.UtcNow.Date;

        var records = await _reconRepo.FindAsync(r => r.TenantId == tenantId, cancellationToken);
        var recordList = records.ToList();

        var exceptions = await _exceptionRepo.FindAsync(e => e.TenantId == tenantId, cancellationToken);
        var exceptionList = exceptions.ToList();

        return new ReconStatsDto(
            TotalRecords: recordList.Count,
            Matched: recordList.Count(r => r.Status == ReconStatus.Matched),
            Mismatched: recordList.Count(r => r.Status == ReconStatus.Mismatched),
            Pending: recordList.Count(r => r.Status == ReconStatus.Pending),
            OpenExceptions: exceptionList.Count(e => e.Status == ExceptionStatus.Open),
            ResolvedToday: exceptionList.Count(e =>
                e.Status == ExceptionStatus.Resolved &&
                e.ResolvedAt.HasValue &&
                e.ResolvedAt.Value.Date == today)
        );
    }
}
