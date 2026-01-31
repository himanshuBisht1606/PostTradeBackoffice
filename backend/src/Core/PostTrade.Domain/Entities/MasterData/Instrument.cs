using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class Instrument : BaseAuditableEntity
{
    public Guid InstrumentId { get; set; }
    public Guid TenantId { get; set; }
    public string InstrumentCode { get; set; } = string.Empty;
    public string InstrumentName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? ISIN { get; set; }
    public Guid ExchangeId { get; set; }
    public Guid SegmentId { get; set; }
    public InstrumentType InstrumentType { get; set; }
    public decimal LotSize { get; set; }
    public decimal TickSize { get; set; }
    public string? Series { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? StrikePrice { get; set; }
    public OptionType? OptionType { get; set; }
    public InstrumentStatus Status { get; set; }
    
    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Exchange Exchange { get; set; } = null!;
    public virtual Segment Segment { get; set; } = null!;
}
