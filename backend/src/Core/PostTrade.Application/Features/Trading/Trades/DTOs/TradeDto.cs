using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Trading.Trades.DTOs;

public record TradeDto(
    Guid TradeId,
    Guid TenantId,
    Guid BrokerId,
    Guid ClientId,
    Guid InstrumentId,
    string TradeNo,
    string? ExchangeTradeNo,
    TradeSide Side,
    int Quantity,
    decimal Price,
    decimal TradeValue,
    DateTime TradeDate,
    DateTime TradeTime,
    string SettlementNo,
    TradeStatus Status,
    string? RejectionReason,
    TradeSource Source,
    decimal Brokerage,
    decimal STT,
    decimal ExchangeTxnCharge,
    decimal GST,
    decimal StampDuty,
    decimal TotalCharges,
    decimal NetAmount
);
