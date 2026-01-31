using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Ledger;

public class ChargesConfiguration : BaseAuditableEntity
{
    public Guid ChargesConfigId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BrokerId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public ChargeType ChargeType { get; set; }
    public CalculationType CalculationType { get; set; }
    public decimal Rate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
