using MediatR;
using PostTrade.Application.Features.Ledger.Charges.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Charges.Queries;

public record GetChargesConfigQuery(
    ChargeType? ChargeType = null,
    bool? IsActive = null
) : IRequest<IEnumerable<ChargesConfigDto>>;

public class GetChargesConfigQueryHandler : IRequestHandler<GetChargesConfigQuery, IEnumerable<ChargesConfigDto>>
{
    private readonly IRepository<ChargesConfiguration> _repo;
    private readonly ITenantContext _tenantContext;

    public GetChargesConfigQueryHandler(IRepository<ChargesConfiguration> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ChargesConfigDto>> Handle(GetChargesConfigQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var configs = await _repo.FindAsync(
            c => c.TenantId == tenantId &&
                 (request.ChargeType == null || c.ChargeType == request.ChargeType) &&
                 (request.IsActive == null || c.IsActive == request.IsActive),
            cancellationToken);

        return configs
            .OrderBy(c => c.ChargeType)
            .ThenBy(c => c.EffectiveFrom)
            .Select(ToDto);
    }

    internal static ChargesConfigDto ToDto(ChargesConfiguration c) => new(
        c.ChargesConfigId, c.TenantId, c.BrokerId,
        c.ChargeName, c.ChargeType, c.CalculationType,
        c.Rate, c.MinAmount, c.MaxAmount,
        c.IsActive, c.EffectiveFrom, c.EffectiveTo);
}
