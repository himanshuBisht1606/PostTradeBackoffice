namespace PostTrade.Application.Features.EOD.DTOs;

public record EodRunResultDto(
    DateTime TradingDate,
    bool Success,
    int PositionsSnapshotted,
    string Message
);
