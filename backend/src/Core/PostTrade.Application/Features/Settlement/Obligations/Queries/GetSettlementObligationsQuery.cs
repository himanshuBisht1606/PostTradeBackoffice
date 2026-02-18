using MediatR;
using PostTrade.Application.Features.Settlement.Obligations.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Obligations.Queries;

public record GetSettlementObligationsQuery(Guid? BatchId = null, ObligationStatus? Status = null)
    : IRequest<IEnumerable<SettlementObligationDto>>;

public class GetSettlementObligationsQueryHandler
    : IRequestHandler<GetSettlementObligationsQuery, IEnumerable<SettlementObligationDto>>
{
    private readonly IRepository<SettlementObligation> _repo;
    private readonly ITenantContext _tenantContext;

    public GetSettlementObligationsQueryHandler(IRepository<SettlementObligation> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<SettlementObligationDto>> Handle(
        GetSettlementObligationsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var obligations = await _repo.FindAsync(
            o => o.TenantId == tenantId &&
                 (request.BatchId == null || o.BatchId == request.BatchId) &&
                 (request.Status == null || o.Status == request.Status),
            cancellationToken);

        return obligations.Select(ToDto);
    }

    internal static SettlementObligationDto ToDto(SettlementObligation o) => new(
        o.ObligationId, o.TenantId, o.BrokerId, o.ClientId, o.BatchId, o.SettlementNo,
        o.FundsPayIn, o.FundsPayOut, o.NetFundsObligation,
        o.SecuritiesPayIn, o.SecuritiesPayOut, o.NetSecuritiesObligation,
        o.Status, o.SettledAt);
}
