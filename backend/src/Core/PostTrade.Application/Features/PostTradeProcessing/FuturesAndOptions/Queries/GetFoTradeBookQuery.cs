using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

public record GetFoTradeBookQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    string? Exchange = null,       // NFO | BFO
    string? Symbol = null,
    string? ClientCode = null,
    string? OptionType = null,     // CE | PE | FX
    string? Side = null,           // B | S
    string? ContractType = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<FoTradeBookPagedDto>;

public class GetFoTradeBookQueryHandler : IRequestHandler<GetFoTradeBookQuery, FoTradeBookPagedDto>
{
    private readonly IRepository<FoTradeBook> _repo;
    private readonly ITenantContext _tenantContext;

    public GetFoTradeBookQueryHandler(IRepository<FoTradeBook> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<FoTradeBookPagedDto> Handle(GetFoTradeBookQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var rows = await _repo.FindAsync(
            t => t.TenantId == tenantId
              && t.TradeDate >= request.DateFrom
              && t.TradeDate <= request.DateTo,
            cancellationToken);

        // Apply optional filters in-memory
        IEnumerable<FoTradeBook> filtered = rows;

        if (!string.IsNullOrWhiteSpace(request.Exchange))
            filtered = filtered.Where(t => t.Exchange.Equals(request.Exchange, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Symbol))
            filtered = filtered.Where(t => t.Symbol.Contains(request.Symbol.Trim(), StringComparison.OrdinalIgnoreCase)
                                        || t.InstrumentName.Contains(request.Symbol.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.ClientCode))
            filtered = filtered.Where(t => t.ClientCode.Contains(request.ClientCode.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.OptionType))
            filtered = filtered.Where(t => t.OptionType.Equals(request.OptionType, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Side))
            filtered = filtered.Where(t => t.Side.Equals(request.Side, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.ContractType))
            filtered = filtered.Where(t => t.ContractType.Equals(request.ContractType, StringComparison.OrdinalIgnoreCase));

        // Sort: newest trade date first, then by symbol
        var sorted = filtered
            .OrderByDescending(t => t.TradeDate)
            .ThenBy(t => t.Symbol)
            .ThenBy(t => t.ExpiryDate)
            .ToList();

        var total = sorted.Count;
        var items = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(Map);

        return new FoTradeBookPagedDto(items, total, request.Page, request.PageSize);
    }

    private static FoTradeBookItemDto Map(FoTradeBook t) => new(
        t.Id,
        t.TradeDate,
        t.Segment,
        t.Exchange,
        t.UniqueTradeId,
        t.ClearingMemberId,
        t.BrokerId,
        t.BranchCode,
        t.Symbol,
        t.InstrumentName,
        t.ContractType,
        t.ExpiryDate,
        t.StrikePrice,
        t.OptionType,
        t.LotSize,
        t.ClientType,
        t.ClientCode,
        t.CtclId,
        t.OriginalClientId,
        t.ClientId,
        t.ClientName,
        t.ClientStateCode,
        t.Side,
        t.Quantity,
        t.NumberOfLots,
        t.Price,
        t.TradeValue,
        t.SettlementType,
        t.SettlementTransactionId,
        t.BatchId
    );
}
