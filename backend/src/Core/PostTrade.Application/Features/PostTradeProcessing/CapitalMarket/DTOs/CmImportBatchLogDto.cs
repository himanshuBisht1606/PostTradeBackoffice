namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;

public record CmImportBatchLogDto(
    Guid LogId,
    Guid BatchId,
    int RowNumber,
    string Level,
    string Message,
    string? RawData
);
