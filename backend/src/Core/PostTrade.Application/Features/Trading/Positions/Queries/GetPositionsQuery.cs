using MediatR;
using PostTrade.Application.Features.Trading.Positions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.Trading.Positions.Queries;

public record GetPositionsQuery : IRequest<IEnumerable<PositionDto>>;

public class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, IEnumerable<PositionDto>>
{
    private readonly IRepository<Position> _repo;
    private readonly ITenantContext _tenantContext;

    public GetPositionsQueryHandler(IRepository<Position> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<PositionDto>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var positions = await _repo.FindAsync(
            p => p.TenantId == tenantId && p.NetQuantity != 0, cancellationToken);
        return positions.Select(ToDto);
    }

    internal static PositionDto ToDto(Position p) => new(
        p.PositionId, p.TenantId, p.ClientId, p.InstrumentId, p.PositionDate,
        p.BuyQuantity, p.SellQuantity, p.NetQuantity, p.CarryForwardQuantity,
        p.AverageBuyPrice, p.AverageSellPrice, p.LastTradePrice, p.MarketPrice,
        p.RealizedPnL, p.UnrealizedPnL, p.DayPnL, p.TotalPnL,
        p.BuyValue, p.SellValue, p.NetValue);
}
