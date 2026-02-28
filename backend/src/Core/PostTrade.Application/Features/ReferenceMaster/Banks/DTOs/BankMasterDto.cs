namespace PostTrade.Application.Features.ReferenceMaster.Banks.DTOs;

public record BankMasterDto(
    Guid BankId,
    string BankCode,
    string BankName,
    string IfscPrefix,   // serialises → "ifscPrefix" (matching TS interface)
    bool IsActive
);
