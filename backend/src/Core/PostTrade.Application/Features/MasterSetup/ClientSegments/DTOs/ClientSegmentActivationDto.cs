using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.ClientSegments.DTOs;

public record ClientSegmentActivationDto(
    Guid ActivationId,
    Guid TenantId,
    Guid ClientId,
    Guid ExchangeSegmentId,
    ActivationStatus Status,
    decimal? ExposureLimit,
    MarginType MarginType,
    DateTime ActivatedOn,
    DateTime? DeactivatedOn
);
