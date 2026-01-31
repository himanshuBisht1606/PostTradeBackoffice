using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Settlement;

public class SettlementBatch : BaseAuditableEntity
{
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }
    public string SettlementNo { get; set; } = string.Empty;
    public DateTime TradeDate { get; set; }
    public DateTime SettlementDate { get; set; }
    public Guid ExchangeId { get; set; }
    public SettlementStatus Status { get; set; }
    public int TotalTrades { get; set; }
    public decimal TotalTurnover { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    
    // Navigation
    public virtual ICollection<SettlementObligation> Obligations { get; set; } = new List<SettlementObligation>();
}
