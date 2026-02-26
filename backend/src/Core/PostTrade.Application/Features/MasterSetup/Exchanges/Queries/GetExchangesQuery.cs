using MediatR;
using PostTrade.Application.Features.MasterSetup.Exchanges.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Exchanges.Queries;

public record GetExchangesQuery : IRequest<IEnumerable<ExchangeDto>>;

public class GetExchangesQueryHandler : IRequestHandler<GetExchangesQuery, IEnumerable<ExchangeDto>>
{
    private readonly IRepository<Exchange> _repo;
    private readonly ITenantContext _tenantContext;

    public GetExchangesQueryHandler(IRepository<Exchange> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ExchangeDto>> Handle(GetExchangesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var exchanges = await _repo.FindAsync(e => e.TenantId == tenantId, cancellationToken);
        return exchanges.Select(e => new ExchangeDto(e.ExchangeId, e.TenantId, e.ExchangeCode, e.ExchangeName,
            e.Country, e.TimeZone, e.TradingStartTime, e.TradingEndTime, e.IsActive));
    }
}
