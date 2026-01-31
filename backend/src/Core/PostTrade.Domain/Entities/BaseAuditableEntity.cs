namespace PostTrade.Domain.Entities;

public abstract class BaseAuditableEntity : BaseEntity
{
    public int Version { get; set; }
    public string? AuditTrail { get; set; }
}
