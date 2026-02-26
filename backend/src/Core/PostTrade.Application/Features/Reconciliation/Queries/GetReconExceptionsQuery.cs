using MediatR;
using PostTrade.Application.Features.Reconciliation.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Reconciliation.Queries;

public record GetReconExceptionsQuery(
    Guid? ReconId = null,
    ExceptionStatus? Status = null
) : IRequest<IEnumerable<ReconExceptionDto>>;

public class GetReconExceptionsQueryHandler : IRequestHandler<GetReconExceptionsQuery, IEnumerable<ReconExceptionDto>>
{
    private readonly IRepository<ReconException> _repo;
    private readonly ITenantContext _tenantContext;

    public GetReconExceptionsQueryHandler(IRepository<ReconException> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ReconExceptionDto>> Handle(GetReconExceptionsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var exceptions = await _repo.FindAsync(
            e => e.TenantId == tenantId &&
                 (request.ReconId == null || e.ReconId == request.ReconId) &&
                 (request.Status == null || e.Status == request.Status),
            cancellationToken);

        return exceptions
            .OrderByDescending(e => e.CreatedAt)
            .Select(ToDto);
    }

    internal static ReconExceptionDto ToDto(ReconException e) => new(
        e.ExceptionId, e.ReconId, e.TenantId,
        e.ExceptionType, e.ExceptionDescription,
        e.ReferenceNo, e.Amount, e.Status,
        e.Resolution, e.ResolvedAt);
}
