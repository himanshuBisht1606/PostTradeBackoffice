using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Instruments.DTOs;

public record InstrumentDto(
    Guid InstrumentId,
    Guid TenantId,
    string InstrumentCode,
    string InstrumentName,
    string Symbol,
    string? ISIN,
    Guid ExchangeId,
    Guid SegmentId,
    InstrumentType InstrumentType,
    decimal LotSize,
    decimal TickSize,
    string? Series,
    DateTime? ExpiryDate,
    decimal? StrikePrice,
    OptionType? OptionType,
    InstrumentStatus Status
);
