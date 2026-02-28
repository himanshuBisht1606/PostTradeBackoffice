namespace PostTrade.Application.Features.ReferenceMaster.BankMappings.DTOs;

public record BankMappingDto(
    Guid MappingId,
    string BankCode,
    string IfscCode,   // serialises → "ifscCode"
    string MicrCode,   // serialises → "micrCode"
    bool IsActive
);
