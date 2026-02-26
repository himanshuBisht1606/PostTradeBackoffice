using MediatR;
using PostTrade.Application.Features.Trading.Trades.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Trading.Trades.Queries;

public record GetTradesQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? ClientId = null,
    Guid? InstrumentId = null,
    TradeStatus? Status = null
) : IRequest<IEnumerable<TradeDto>>;

public class GetTradesQueryHandler : IRequestHandler<GetTradesQuery, IEnumerable<TradeDto>>
{
    private readonly IRepository<Trade> _repo;
    private readonly ITenantContext _tenantContext;

    public GetTradesQueryHandler(IRepository<Trade> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<TradeDto>> Handle(GetTradesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var trades = await _repo.FindAsync(t =>
            t.TenantId == tenantId &&
            (request.FromDate == null || t.TradeDate >= request.FromDate) &&
            (request.ToDate == null || t.TradeDate <= request.ToDate) &&
            (request.ClientId == null || t.ClientId == request.ClientId) &&
            (request.InstrumentId == null || t.InstrumentId == request.InstrumentId) &&
            (request.Status == null || t.Status == request.Status),
            cancellationToken);

        return trades.Select(ToDto);
    }

    internal static TradeDto ToDto(Trade t) => new(
        t.TradeId, t.TenantId, t.BrokerId, t.ClientId, t.InstrumentId,
        t.TradeNo, t.ExchangeTradeNo, t.Side, t.Quantity, t.Price,
        t.TradeValue, t.TradeDate, t.TradeTime, t.SettlementNo, t.Status,
        t.RejectionReason, t.Source,
        t.Brokerage, t.STT, t.ExchangeTxnCharge, t.GST, t.StampDuty,
        t.TotalCharges, t.NetAmount);
}
