namespace PostTrade.Application.Features.Trading.PnL.DTOs;

public record PnLSnapshotDto(
    Guid PnLId,
    Guid TenantId,
    Guid ClientId,
    Guid InstrumentId,
    DateTime SnapshotDate,
    DateTime SnapshotTime,
    decimal RealizedPnL,
    decimal UnrealizedPnL,
    decimal TotalPnL,
    decimal Brokerage,
    decimal Taxes,
    decimal NetPnL,
    int OpenQuantity,
    decimal AveragePrice,
    decimal MarketPrice
);
