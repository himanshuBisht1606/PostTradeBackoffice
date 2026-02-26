namespace PostTrade.Application.Features.Trading.Positions.DTOs;

public record PositionDto(
    Guid PositionId,
    Guid TenantId,
    Guid ClientId,
    Guid InstrumentId,
    DateTime PositionDate,
    int BuyQuantity,
    int SellQuantity,
    int NetQuantity,
    int CarryForwardQuantity,
    decimal AverageBuyPrice,
    decimal AverageSellPrice,
    decimal LastTradePrice,
    decimal MarketPrice,
    decimal RealizedPnL,
    decimal UnrealizedPnL,
    decimal DayPnL,
    decimal TotalPnL,
    decimal BuyValue,
    decimal SellValue,
    decimal NetValue
);
