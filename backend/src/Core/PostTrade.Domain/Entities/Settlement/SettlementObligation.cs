using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Settlement;

public class SettlementObligation : BaseAuditableEntity
{
    public Guid ObligationId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid BatchId { get; set; }
    public string SettlementNo { get; set; } = string.Empty;
    
    // Funds Obligations
    public decimal FundsPayIn { get; set; }
    public decimal FundsPayOut { get; set; }
    public decimal NetFundsObligation { get; set; }
    
    // Securities Obligations
    public int SecuritiesPayIn { get; set; }
    public int SecuritiesPayOut { get; set; }
    public int NetSecuritiesObligation { get; set; }
    
    public ObligationStatus Status { get; set; }
    public DateTime? SettledAt { get; set; }
    
    // Navigation
    public virtual SettlementBatch Batch { get; set; } = null!;
}
