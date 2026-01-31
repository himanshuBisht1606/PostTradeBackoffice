namespace PostTrade.Application.Interfaces;
public interface ITradeRepository
{
    Task AddAsync(object trade);
}
