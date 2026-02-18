using MediatR;
using PostTrade.Application.Features.MasterSetup.Exchanges.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Exchanges.Queries;

public record GetExchangeByIdQuery(Guid ExchangeId) : IRequest<ExchangeDto?>;

public class GetExchangeByIdQueryHandler : IRequestHandler<GetExchangeByIdQuery, ExchangeDto?>
{
    private readonly IRepository<Exchange> _repo;
    private readonly ITenantContext _tenantContext;

    public GetExchangeByIdQueryHandler(IRepository<Exchange> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<ExchangeDto?> Handle(GetExchangeByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(e => e.ExchangeId == request.ExchangeId && e.TenantId == tenantId, cancellationToken);
        var e = results.FirstOrDefault();
        if (e is null) return null;
        return new ExchangeDto(e.ExchangeId, e.TenantId, e.ExchangeCode, e.ExchangeName,
            e.Country, e.TimeZone, e.TradingStartTime, e.TradingEndTime, e.IsActive);
    }
}
