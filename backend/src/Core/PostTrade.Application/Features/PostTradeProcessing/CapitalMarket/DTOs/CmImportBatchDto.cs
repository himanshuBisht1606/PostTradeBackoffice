using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;

public record CmImportBatchDto(
    Guid BatchId,
    Guid TenantId,
    CmFileType FileType,
    string Exchange,
    DateOnly TradingDate,
    CmImportStatus Status,
    CmTriggerSource TriggerSource,
    string FileName,
    int TotalRows,
    int CreatedRows,
    int SkippedRows,
    int ErrorRows,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage
);
