using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Charges.DTOs;

public record ChargesConfigDto(
    Guid ChargesConfigId,
    Guid TenantId,
    Guid? BrokerId,
    string ChargeName,
    ChargeType ChargeType,
    CalculationType CalculationType,
    decimal Rate,
    decimal? MinAmount,
    decimal? MaxAmount,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);
