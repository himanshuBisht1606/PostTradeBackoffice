using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Batches.DTOs;

public record SettlementBatchDto(
    Guid BatchId,
    Guid TenantId,
    string SettlementNo,
    DateTime TradeDate,
    DateTime SettlementDate,
    Guid ExchangeId,
    SettlementStatus Status,
    int TotalTrades,
    decimal TotalTurnover,
    DateTime? ProcessedAt,
    string? ProcessedBy
);
