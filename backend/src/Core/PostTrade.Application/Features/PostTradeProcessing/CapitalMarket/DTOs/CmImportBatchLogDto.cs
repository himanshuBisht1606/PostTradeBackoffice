namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;

public record CmImportBatchLogDto(
    Guid LogId,
    Guid BatchId,
    int RowNumber,
    string Level,
    string Message,
    string? RawData
);

public record CmImportBatchLogSummaryItemDto(
    string Level,
    string Message,
    int Count
);

public record CmImportBatchLogsPagedDto(
    IEnumerable<CmImportBatchLogSummaryItemDto> Summary,
    IEnumerable<CmImportBatchLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
