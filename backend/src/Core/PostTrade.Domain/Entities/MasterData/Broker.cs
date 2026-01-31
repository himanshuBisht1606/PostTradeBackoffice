using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class Broker : BaseAuditableEntity
{
    public Guid BrokerId { get; set; }
    public Guid TenantId { get; set; }
    public string BrokerCode { get; set; } = string.Empty;
    public string BrokerName { get; set; } = string.Empty;
    public string? SEBIRegistrationNo { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public BrokerStatus Status { get; set; }
    public string? Address { get; set; }
    public string? PAN { get; set; }
    public string? GST { get; set; }
    
    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
