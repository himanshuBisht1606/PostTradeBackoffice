namespace PostTrade.Domain.Entities.MasterData;

// Global reference table — no TenantId
public class StateMaster : BaseEntity
{
    public Guid StateId { get; set; }
    public string CountryId { get; set; } = "IN";
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public int? NseCode { get; set; }
    public string? BseName { get; set; }
    public int? CvlCode { get; set; }
    public int? NdmlCode { get; set; }
    public int? NcdexCode { get; set; }
    public int? NseKraCode { get; set; }
    public int? NsdlCode { get; set; }
    public bool IsActive { get; set; } = true;
}
