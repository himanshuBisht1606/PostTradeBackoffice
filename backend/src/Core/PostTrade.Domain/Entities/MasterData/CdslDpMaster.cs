namespace PostTrade.Domain.Entities.MasterData;

// Global reference table — no TenantId
public class CdslDpMaster : BaseEntity
{
    public Guid DpId { get; set; }
    public string DpCode { get; set; } = string.Empty;       // numeric string e.g. "10000"
    public string DpName { get; set; } = string.Empty;
    public string? SebiRegNo { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string MemberStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
