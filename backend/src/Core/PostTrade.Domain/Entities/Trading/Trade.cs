using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Trading;

public class Trade : BaseAuditableEntity
{
    public Guid TradeId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid InstrumentId { get; set; }
    public string TradeNo { get; set; } = string.Empty;
    public string? ExchangeTradeNo { get; set; }
    public TradeSide Side { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TradeValue { get; set; }
    public DateTime TradeDate { get; set; }
    public DateTime TradeTime { get; set; }
    public string SettlementNo { get; set; } = string.Empty;
    public TradeStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public TradeSource Source { get; set; }
    public string? SourceReference { get; set; }
    public bool IsAmended { get; set; }
    public Guid? OriginalTradeId { get; set; }
    
    // Charges
    public decimal Brokerage { get; set; }
    public decimal STT { get; set; }
    public decimal ExchangeTxnCharge { get; set; }
    public decimal GST { get; set; }
    public decimal SEBITurnoverCharge { get; set; }
    public decimal StampDuty { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal NetAmount { get; set; }
}
