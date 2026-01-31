using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Reconciliation;

public class ReconException : BaseAuditableEntity
{
    public Guid ExceptionId { get; set; }
    public Guid ReconId { get; set; }
    public Guid TenantId { get; set; }
    public ExceptionType ExceptionType { get; set; }
    public string ExceptionDescription { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExceptionStatus Status { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
