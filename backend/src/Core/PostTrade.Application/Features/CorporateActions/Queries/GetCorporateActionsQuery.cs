using MediatR;
using PostTrade.Application.Features.CorporateActions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.CorporateActions.Queries;

public record GetCorporateActionsQuery(
    Guid? InstrumentId = null,
    CorporateActionType? ActionType = null,
    CorporateActionStatus? Status = null
) : IRequest<IEnumerable<CorporateActionDto>>;

public class GetCorporateActionsQueryHandler : IRequestHandler<GetCorporateActionsQuery, IEnumerable<CorporateActionDto>>
{
    private readonly IRepository<CorporateAction> _repo;
    private readonly ITenantContext _tenantContext;

    public GetCorporateActionsQueryHandler(IRepository<CorporateAction> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CorporateActionDto>> Handle(GetCorporateActionsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var actions = await _repo.FindAsync(
            a => a.TenantId == tenantId &&
                 (request.InstrumentId == null || a.InstrumentId == request.InstrumentId) &&
                 (request.ActionType == null || a.ActionType == request.ActionType) &&
                 (request.Status == null || a.Status == request.Status),
            cancellationToken);

        return actions
            .OrderByDescending(a => a.AnnouncementDate)
            .Select(ToDto);
    }

    internal static CorporateActionDto ToDto(CorporateAction a) => new(
        a.CorporateActionId, a.TenantId, a.InstrumentId,
        a.ActionType, a.AnnouncementDate, a.ExDate, a.RecordDate, a.PaymentDate,
        a.DividendAmount, a.BonusRatio, a.SplitRatio, a.RightsRatio, a.RightsPrice,
        a.Status, a.IsProcessed, a.ProcessedAt);
}
