using MediatR;
using PostTrade.Application.Features.Trading.Trades.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.Trading.Trades.Queries;

public record GetTradeByIdQuery(Guid TradeId) : IRequest<TradeDto?>;

public class GetTradeByIdQueryHandler : IRequestHandler<GetTradeByIdQuery, TradeDto?>
{
    private readonly IRepository<Trade> _repo;
    private readonly ITenantContext _tenantContext;

    public GetTradeByIdQueryHandler(IRepository<Trade> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<TradeDto?> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            t => t.TradeId == request.TradeId && t.TenantId == tenantId, cancellationToken);
        var trade = results.FirstOrDefault();
        return trade is null ? null : GetTradesQueryHandler.ToDto(trade);
    }
}
