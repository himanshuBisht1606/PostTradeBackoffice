namespace PostTrade.Application.Features.MasterSetup.Segments.DTOs;

public record SegmentDto(
    Guid SegmentId,
    Guid TenantId,
    Guid ExchangeId,
    string SegmentCode,
    string SegmentName,
    bool IsActive
);
