namespace PostTrade.Domain.Entities.MasterData;

public class Exchange : BaseEntity
{
    public Guid ExchangeId { get; set; }
    public Guid TenantId { get; set; }
    public string ExchangeCode { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public TimeOnly? TradingStartTime { get; set; }
    public TimeOnly? TradingEndTime { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Segment> Segments { get; set; } = new List<Segment>();
}
