using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoImportBatchDto(
    Guid BatchId,
    Guid TenantId,
    FoFileType FileType,
    string Exchange,
    DateOnly TradingDate,
    FoImportStatus Status,
    FoTriggerSource TriggerSource,
    string FileName,
    int TotalRows,
    int CreatedRows,
    int SkippedRows,
    int ErrorRows,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage
);
