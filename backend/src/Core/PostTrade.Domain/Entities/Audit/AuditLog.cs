using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Audit;

public class AuditLog : BaseEntity
{
    public Guid AuditId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public AuditType AuditType { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
