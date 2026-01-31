using PostTrade.Domain.Enums;
using PostTrade.Domain.Exceptions;

namespace PostTrade.Domain.Entities;

public class Trade : BaseEntity
{
    public Guid TradeId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ClientId { get; private set; }
    public TradeSide Side { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime TradeTime { get; private set; }
    public TradeStatus Status { get; private set; }

    private Trade() { }

    public static Trade Create(
        Guid tenantId, Guid clientId, TradeSide side,
        int quantity, decimal price, DateTime tradeTime)
    {
        if (quantity <= 0) throw new DomainException("Invalid quantity");
        if (price < 0) throw new DomainException("Invalid price");

        return new Trade
        {
            TradeId = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = clientId,
            Side = side,
            Quantity = quantity,
            Price = price,
            TradeTime = tradeTime,
            Status = TradeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}
