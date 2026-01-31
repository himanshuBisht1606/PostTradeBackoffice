namespace PostTrade.Domain.Entities.Trading;

public class PnLSnapshot : BaseEntity
{
    public Guid PnLId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid InstrumentId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public DateTime SnapshotTime { get; set; }
    
    public decimal RealizedPnL { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal TotalPnL { get; set; }
    public decimal Brokerage { get; set; }
    public decimal Taxes { get; set; }
    public decimal NetPnL { get; set; }
    
    public int OpenQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MarketPrice { get; set; }
}
