namespace PostTrade.Application.Features.MasterSetup.Exchanges.DTOs;

public record ExchangeDto(
    Guid ExchangeId,
    Guid TenantId,
    string ExchangeCode,
    string ExchangeName,
    string Country,
    string? TimeZone,
    TimeOnly? TradingStartTime,
    TimeOnly? TradingEndTime,
    bool IsActive
);
