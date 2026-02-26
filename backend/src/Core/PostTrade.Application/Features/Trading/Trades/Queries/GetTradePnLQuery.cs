using MediatR;
using PostTrade.Application.Features.Trading.PnL.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.Trading.Trades.Queries;

public record GetTradePnLQuery(Guid TradeId) : IRequest<PnLSnapshotDto?>;

public class GetTradePnLQueryHandler : IRequestHandler<GetTradePnLQuery, PnLSnapshotDto?>
{
    private readonly IRepository<Trade> _tradeRepo;
    private readonly IRepository<PnLSnapshot> _pnlRepo;
    private readonly ITenantContext _tenantContext;

    public GetTradePnLQueryHandler(IRepository<Trade> tradeRepo, IRepository<PnLSnapshot> pnlRepo, ITenantContext tenantContext)
    {
        _tradeRepo = tradeRepo;
        _pnlRepo = pnlRepo;
        _tenantContext = tenantContext;
    }

    public async Task<PnLSnapshotDto?> Handle(GetTradePnLQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var trades = await _tradeRepo.FindAsync(
            t => t.TradeId == request.TradeId && t.TenantId == tenantId, cancellationToken);
        var trade = trades.FirstOrDefault();
        if (trade is null) return null;

        var snapshots = await _pnlRepo.FindAsync(
            p => p.ClientId == trade.ClientId && p.InstrumentId == trade.InstrumentId && p.TenantId == tenantId,
            cancellationToken);

        var latest = snapshots.OrderByDescending(p => p.SnapshotTime).FirstOrDefault();
        if (latest is null) return null;

        return new PnLSnapshotDto(latest.PnLId, latest.TenantId, latest.ClientId, latest.InstrumentId,
            latest.SnapshotDate, latest.SnapshotTime, latest.RealizedPnL, latest.UnrealizedPnL,
            latest.TotalPnL, latest.Brokerage, latest.Taxes, latest.NetPnL,
            latest.OpenQuantity, latest.AveragePrice, latest.MarketPrice);
    }
}
