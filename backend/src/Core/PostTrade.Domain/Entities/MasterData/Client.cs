using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class Client : BaseAuditableEntity
{
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public ClientType ClientType { get; set; }
    public ClientStatus Status { get; set; }
    public string? PAN { get; set; }
    public string? DPId { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankName { get; set; }
    public string? BankIFSC { get; set; }
    public string? Address { get; set; }
    
    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Broker Broker { get; set; } = null!;
}
