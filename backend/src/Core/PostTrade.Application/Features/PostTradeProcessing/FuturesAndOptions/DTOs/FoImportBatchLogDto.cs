namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoImportBatchLogDto(
    Guid LogId,
    Guid BatchId,
    int RowNumber,
    string Level,
    string Message,
    string? RawData
);
