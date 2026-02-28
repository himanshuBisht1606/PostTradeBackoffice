using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class Client : BaseAuditableEntity
{
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid? BranchId { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public ClientType ClientType { get; set; }
    public ClientStatus Status { get; set; }
    public string? PAN { get; set; }
    public string? Aadhaar { get; set; }
    public string? DPId { get; set; }
    public string? DematAccountNo { get; set; }
    public Depository? Depository { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankName { get; set; }
    public string? BankIFSC { get; set; }
    public string? Address { get; set; }
    public string? StateCode { get; set; }
    public string? StateName { get; set; }
    public KYCStatus KYCStatus { get; set; }
    public RiskCategory RiskCategory { get; set; }

    // Onboarding — personal details
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Occupation { get; set; }
    public string? GrossAnnualIncome { get; set; }
    public string? FatherSpouseName { get; set; }
    public string? MotherName { get; set; }

    // Onboarding — extended contact & address
    public string? AlternateMobile { get; set; }
    public string? City { get; set; }
    public string? PinCode { get; set; }
    public string? CorrespondenceAddress { get; set; }

    // Onboarding — holder type & identity
    public string HolderType { get; set; } = "Single";
    public string? CitizenshipStatus { get; set; }
    public string? ResidentialStatus { get; set; }
    public string? IdentityProofType { get; set; }
    public string? IdentityProofNumber { get; set; }

    // Onboarding — bank details
    public string? AccountType { get; set; }
    public string? BranchName { get; set; }

    // Onboarding — trading segments
    public bool SegmentNSE { get; set; }
    public bool SegmentBSE { get; set; }
    public bool SegmentMCX { get; set; }

    // Non-individual entity fields
    public string? EntityRegistrationNumber { get; set; }
    public DateOnly? DateOfConstitution { get; set; }
    public string? ConstitutionType { get; set; }
    public string? GSTNumber { get; set; }
    public string? AnnualTurnover { get; set; }
    public string? KartaName { get; set; }
    public string? KartaPan { get; set; }

    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Broker Broker { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<ClientSegmentActivation> SegmentActivations { get; set; } = new List<ClientSegmentActivation>();
    public virtual ICollection<ClientNominee> Nominees { get; set; } = new List<ClientNominee>();
    public virtual ICollection<ClientAuthorizedSignatory> AuthorizedSignatories { get; set; } = new List<ClientAuthorizedSignatory>();
}
