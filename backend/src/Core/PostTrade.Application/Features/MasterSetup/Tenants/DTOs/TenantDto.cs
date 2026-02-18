using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Tenants.DTOs;

public record TenantDto(
    Guid TenantId,
    string TenantCode,
    string TenantName,
    string ContactEmail,
    string ContactPhone,
    TenantStatus Status,
    string? Address,
    string? City,
    string? Country,
    string? TaxId,
    DateTime? LicenseExpiryDate
);
