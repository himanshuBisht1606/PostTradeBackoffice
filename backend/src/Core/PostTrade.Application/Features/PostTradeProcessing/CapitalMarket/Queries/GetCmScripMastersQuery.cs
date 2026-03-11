using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Queries;

public record GetCmScripMastersQuery(
    string? Exchange,
    DateOnly? TradingDate,
    string? Symbol,
    string? ISIN,
    int Page = 1,
    int PageSize = 50
) : IRequest<IEnumerable<CmScripMasterDto>>;

public class GetCmScripMastersQueryHandler : IRequestHandler<GetCmScripMastersQuery, IEnumerable<CmScripMasterDto>>
{
    private readonly IRepository<CmScripMaster> _repo;
    private readonly ITenantContext _tenantContext;

    public GetCmScripMastersQueryHandler(IRepository<CmScripMaster> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CmScripMasterDto>> Handle(GetCmScripMastersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var records = await _repo.FindAsync(s => s.TenantId == tenantId, cancellationToken);

        var query = records.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Exchange))
            query = query.Where(s => s.Exchange == request.Exchange);

        if (request.TradingDate.HasValue)
            query = query.Where(s => s.TradingDate == request.TradingDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Symbol))
            query = query.Where(s => s.Symbol.Contains(request.Symbol, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.ISIN))
            query = query.Where(s => s.ISIN.Equals(request.ISIN, StringComparison.OrdinalIgnoreCase));

        return query
            .OrderBy(s => s.Symbol)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new CmScripMasterDto(
                s.CmScripMasterId, s.Exchange, s.TradingDate,
                s.Symbol, s.ISIN, s.Series, s.Name,
                s.FaceValue, s.LotSize, s.TickSize, s.InstrumentType))
            .ToList();
    }
}
