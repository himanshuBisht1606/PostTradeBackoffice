namespace PostTrade.Application.Interfaces;

public interface IEODProcessingService
{
    Task<bool> ProcessEndOfDayAsync(DateTime tradingDate);
}
