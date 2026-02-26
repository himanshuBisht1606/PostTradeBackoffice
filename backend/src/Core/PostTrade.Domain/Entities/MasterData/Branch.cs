namespace PostTrade.Domain.Entities.MasterData;

public class Branch : BaseEntity
{
    public Guid BranchId { get; set; }
    public Guid TenantId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
