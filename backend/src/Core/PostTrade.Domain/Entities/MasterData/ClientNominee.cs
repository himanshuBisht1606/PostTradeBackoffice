namespace PostTrade.Domain.Entities.MasterData;

public class ClientNominee : BaseEntity
{
    public Guid NomineeId { get; set; }
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }

    public string NomineeName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? NomineePAN { get; set; }
    public decimal SharePercentage { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    // Navigation
    public virtual Client Client { get; set; } = null!;
}
