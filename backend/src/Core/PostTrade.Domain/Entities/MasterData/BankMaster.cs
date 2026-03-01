namespace PostTrade.Domain.Entities.MasterData;

// Global reference table — no TenantId
public class BankMaster : BaseEntity
{
    public Guid BankId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IFSCPrefix { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
