using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class Tenant : BaseAuditableEntity
{
    public Guid TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public string? LicenseKey { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TaxId { get; set; }
    
    // Navigation
    public virtual ICollection<Broker> Brokers { get; set; } = new List<Broker>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
