using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Ledger;

public class LedgerEntry : BaseAuditableEntity
{
    public Guid LedgerId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid ClientId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public DateTime ValueDate { get; set; }
    
    public LedgerType LedgerType { get; set; }
    public EntryType EntryType { get; set; }
    
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    
    public string ReferenceType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string? Narration { get; set; }
    
    public bool IsReversed { get; set; }
    public Guid? ReversalLedgerId { get; set; }
}
