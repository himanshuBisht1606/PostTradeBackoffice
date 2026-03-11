namespace PostTrade.Domain.Entities.MasterData;

// Global reference table — no TenantId
public class NsdlDpMaster : BaseEntity
{
    public Guid DpId { get; set; }
    public string DpCode { get; set; } = string.Empty;       // e.g. "IN001002"
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
