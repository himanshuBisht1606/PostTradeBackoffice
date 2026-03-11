using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Queries;

public record GetCmSettlementMastersQuery(
    string? Exchange,
    DateOnly? TradingDate
) : IRequest<IEnumerable<CmSettlementMasterDto>>;

public class GetCmSettlementMastersQueryHandler : IRequestHandler<GetCmSettlementMastersQuery, IEnumerable<CmSettlementMasterDto>>
{
    private readonly IRepository<CmSettlementMaster> _repo;
    private readonly ITenantContext _tenantContext;

    public GetCmSettlementMastersQueryHandler(IRepository<CmSettlementMaster> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CmSettlementMasterDto>> Handle(GetCmSettlementMastersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var records = await _repo.FindAsync(s => s.TenantId == tenantId, cancellationToken);

        var query = records.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Exchange))
            query = query.Where(s => s.Exchange == request.Exchange);

        if (request.TradingDate.HasValue)
            query = query.Where(s => s.TradingDate == request.TradingDate.Value);

        return query
            .OrderBy(s => s.TradingDate)
            .ThenBy(s => s.Exchange)
            .ThenBy(s => s.SettlementNo)
            .Select(s => new CmSettlementMasterDto(
                s.CmSettlementMasterId, s.Exchange, s.TradingDate,
                s.SettlementNo, s.SettlementType, s.PayInDate, s.PayOutDate))
            .ToList();
    }
}
