using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.DTOs;

/// <summary>Full broker profile returned by GET /api/brokers/{id}.</summary>
public record BrokerDetailDto(
    Guid BrokerId,
    Guid TenantId,

    // Identity
    string BrokerCode,
    string BrokerName,
    BrokerEntityType EntityType,
    BrokerStatus Status,
    string? LogoUrl,
    string? Website,

    // Company / Corporate
    string? CIN,
    string? TAN,
    string? PAN,
    string? GST,
    DateOnly? IncorporationDate,

    // Contact
    string ContactEmail,
    string ContactPhone,

    // Registered Address
    string? RegisteredAddressLine1,
    string? RegisteredAddressLine2,
    string? RegisteredCity,
    string? RegisteredState,
    string? RegisteredPinCode,
    string? RegisteredCountry,

    // Correspondence Address
    bool CorrespondenceSameAsRegistered,
    string? CorrespondenceAddressLine1,
    string? CorrespondenceAddressLine2,
    string? CorrespondenceCity,
    string? CorrespondenceState,
    string? CorrespondencePinCode,

    // SEBI
    string? SEBIRegistrationNo,
    DateOnly? SEBIRegistrationDate,
    DateOnly? SEBIRegistrationExpiry,

    // Compliance
    string? ComplianceOfficerName,
    string? ComplianceOfficerEmail,
    string? ComplianceOfficerPhone,
    string? PrincipalOfficerName,
    string? PrincipalOfficerEmail,
    string? PrincipalOfficerPhone,

    // Settlement Bank
    string? SettlementBankName,
    string? SettlementBankAccountNo,
    string? SettlementBankIfsc,
    string? SettlementBankBranch,

    // Exchange Memberships
    IEnumerable<BrokerExchangeMembershipDto> ExchangeMemberships
);
