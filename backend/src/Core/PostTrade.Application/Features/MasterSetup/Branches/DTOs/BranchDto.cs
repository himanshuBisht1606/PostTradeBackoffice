namespace PostTrade.Application.Features.MasterSetup.Branches.DTOs;

public record BranchDto(
    Guid BranchId,
    Guid TenantId,
    string BranchCode,
    string BranchName,
    string? Address,
    string? City,
    string StateCode,
    string StateName,
    string? GSTIN,
    string? ContactPerson,
    string? ContactPhone,
    string? ContactEmail,
    bool IsActive
);
