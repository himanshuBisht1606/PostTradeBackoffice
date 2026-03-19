namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoImportBatchLogDto(
    Guid LogId,
    Guid BatchId,
    int RowNumber,
    string Level,
    string Message,
    string? RawData
);

public record FoImportBatchLogSummaryItemDto(
    string Level,
    string Message,
    int Count
);

public record FoImportBatchLogsPagedDto(
    IEnumerable<FoImportBatchLogSummaryItemDto> Summary,
    IEnumerable<FoImportBatchLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
