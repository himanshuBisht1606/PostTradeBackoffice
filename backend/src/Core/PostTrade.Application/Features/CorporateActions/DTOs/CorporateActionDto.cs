using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.CorporateActions.DTOs;

public record CorporateActionDto(
    Guid CorporateActionId,
    Guid TenantId,
    Guid InstrumentId,
    CorporateActionType ActionType,
    DateTime AnnouncementDate,
    DateTime ExDate,
    DateTime RecordDate,
    DateTime? PaymentDate,
    decimal? DividendAmount,
    decimal? BonusRatio,
    decimal? SplitRatio,
    decimal? RightsRatio,
    decimal? RightsPrice,
    CorporateActionStatus Status,
    bool IsProcessed,
    DateTime? ProcessedAt
);
