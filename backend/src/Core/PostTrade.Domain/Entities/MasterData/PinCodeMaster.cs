namespace PostTrade.Domain.Entities.MasterData;

// Global reference table — no TenantId
public class PinCodeMaster : BaseEntity
{
    public Guid PinCodeId { get; set; }
    public string PinCode { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? City { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? McxCode { get; set; }
    public bool IsActive { get; set; } = true;
}
