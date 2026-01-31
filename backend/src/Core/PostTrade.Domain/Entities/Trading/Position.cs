namespace PostTrade.Domain.Entities.Trading;

public class Position : BaseAuditableEntity
{
    public Guid PositionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid InstrumentId { get; set; }
    public DateTime PositionDate { get; set; }
    
    // Quantities
    public int BuyQuantity { get; set; }
    public int SellQuantity { get; set; }
    public int NetQuantity { get; set; }
    public int CarryForwardQuantity { get; set; }
    
    // Prices
    public decimal AverageBuyPrice { get; set; }
    public decimal AverageSellPrice { get; set; }
    public decimal LastTradePrice { get; set; }
    public decimal MarketPrice { get; set; }
    
    // PnL
    public decimal RealizedPnL { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal DayPnL { get; set; }
    public decimal TotalPnL { get; set; }
    
    // Values
    public decimal BuyValue { get; set; }
    public decimal SellValue { get; set; }
    public decimal NetValue { get; set; }
}
