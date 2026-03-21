namespace PostTrade.Application.Features.MasterSetup.Segments.DTOs;

public record SegmentDto(
    Guid SegmentId,
    Guid TenantId,
    string SegmentCode,
    string SegmentName,
    string? Description,
    bool IsActive
);
