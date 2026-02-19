namespace PostTrade.Application.Features.EOD.DTOs;

public record EodStatusDto(
    DateTime TradingDate,
    bool IsProcessed,
    int SnapshotCount,
    DateTime? ProcessedAt
);
