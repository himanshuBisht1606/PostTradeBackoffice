namespace PostTrade.Application.Features.Trades.DTOs;

public class TradeDto
{
    public Guid TradeId { get; set; }
    public Guid TenantId { get; set; }
    public string Side { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
