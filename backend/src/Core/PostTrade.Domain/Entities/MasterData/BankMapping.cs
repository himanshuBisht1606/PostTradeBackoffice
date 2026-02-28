namespace PostTrade.Domain.Entities.MasterData;

// Global reference table — no TenantId
// BankCode is a plain string — no FK to BankMaster (avoids integrity issues on large import)
public class BankMapping : BaseEntity
{
    public Guid MappingId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string MICRCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
