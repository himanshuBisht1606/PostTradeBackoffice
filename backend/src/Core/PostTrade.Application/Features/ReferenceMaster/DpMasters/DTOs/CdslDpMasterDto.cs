namespace PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;

public record CdslDpMasterDto(
    Guid DpId,
    string DpCode,
    string DpName,
    string? SebiRegNo,
    string? City,
    string? State,
    string? PinCode,
    string? Phone,
    string? Email,
    string MemberStatus,
    bool IsActive
);
