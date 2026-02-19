using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Reconciliation.DTOs;

public record ReconciliationDto(
    Guid ReconId,
    Guid TenantId,
    DateTime ReconDate,
    string SettlementNo,
    ReconType ReconType,
    decimal SystemValue,
    decimal ExchangeValue,
    decimal Difference,
    decimal ToleranceLimit,
    ReconStatus Status,
    string? Comments,
    DateTime? ResolvedAt,
    string? ResolvedBy
);
