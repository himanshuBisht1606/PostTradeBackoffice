namespace PostTrade.Application.Features.ReferenceMaster.PinCodes.DTOs;

public record PinCodeMasterDto(
    Guid PinCodeId,
    string PinCode,
    string? District,
    string? City,
    string StateCode,
    string CountryCode,
    string? McxCode,
    bool IsActive
);
