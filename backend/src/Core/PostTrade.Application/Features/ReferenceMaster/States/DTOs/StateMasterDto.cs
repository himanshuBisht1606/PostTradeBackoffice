namespace PostTrade.Application.Features.ReferenceMaster.States.DTOs;

public record StateMasterDto(
    Guid StateId,
    string CountryId,
    string StateCode,
    string StateName,
    int? NseCode,
    string? BseName,
    int? CvlCode,
    int? NdmlCode,
    int? NcdexCode,
    int? NseKraCode,
    int? NsdlCode,
    bool IsActive
);
