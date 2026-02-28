using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.ExchangeSegments.DTOs;

public record ExchangeSegmentDto(
    Guid ExchangeSegmentId,
    Guid TenantId,
    Guid ExchangeId,
    Guid SegmentId,
    string ExchangeSegmentCode,
    string ExchangeSegmentName,
    SettlementType SettlementType,
    bool IsActive
);
