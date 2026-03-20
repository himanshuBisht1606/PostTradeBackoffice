using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class Broker : BaseAuditableEntity
{
    public Guid BrokerId { get; set; }
    public Guid TenantId { get; set; }

    // ── Identity ────────────────────────────────────────────────────────────────
    public string BrokerCode { get; set; } = string.Empty;
    public string BrokerName { get; set; } = string.Empty;
    public BrokerEntityType EntityType { get; set; }
    public BrokerStatus Status { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }

    // ── Company / Corporate Details ─────────────────────────────────────────────
    public string? CIN { get; set; }                      // Corporate Identification Number
    public string? TAN { get; set; }                      // Tax Deduction Account Number
    public string? PAN { get; set; }
    public string? GST { get; set; }
    public DateOnly? IncorporationDate { get; set; }

    // ── Contact ─────────────────────────────────────────────────────────────────
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    // ── Registered Address ───────────────────────────────────────────────────────
    public string? RegisteredAddressLine1 { get; set; }
    public string? RegisteredAddressLine2 { get; set; }
    public string? RegisteredCity { get; set; }
    public string? RegisteredState { get; set; }
    public string? RegisteredPinCode { get; set; }
    public string? RegisteredCountry { get; set; } = "India";

    // ── Correspondence Address (if different from registered) ────────────────────
    public bool CorrespondenceSameAsRegistered { get; set; } = true;
    public string? CorrespondenceAddressLine1 { get; set; }
    public string? CorrespondenceAddressLine2 { get; set; }
    public string? CorrespondenceCity { get; set; }
    public string? CorrespondenceState { get; set; }
    public string? CorrespondencePinCode { get; set; }

    // ── SEBI Registration ────────────────────────────────────────────────────────
    public string? SEBIRegistrationNo { get; set; }
    public DateOnly? SEBIRegistrationDate { get; set; }
    public DateOnly? SEBIRegistrationExpiry { get; set; }

    // ── Compliance Officers ──────────────────────────────────────────────────────
    public string? ComplianceOfficerName { get; set; }
    public string? ComplianceOfficerEmail { get; set; }
    public string? ComplianceOfficerPhone { get; set; }
    public string? PrincipalOfficerName { get; set; }
    public string? PrincipalOfficerEmail { get; set; }
    public string? PrincipalOfficerPhone { get; set; }

    // ── Settlement Bank ──────────────────────────────────────────────────────────
    public string? SettlementBankName { get; set; }
    public string? SettlementBankAccountNo { get; set; }
    public string? SettlementBankIfsc { get; set; }
    public string? SettlementBankBranch { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────────
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    public virtual ICollection<BrokerExchangeMembership> ExchangeMemberships { get; set; } = new List<BrokerExchangeMembership>();
}
