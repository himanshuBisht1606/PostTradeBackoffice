using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

public record GetFoFinanceLedgerQuery(
    DateOnly? TradeDate = null,
    string? Exchange = null,
    string? ClientCode = null
) : IRequest<IEnumerable<FoFinanceLedgerDto>>;

public class GetFoFinanceLedgerQueryHandler : IRequestHandler<GetFoFinanceLedgerQuery, IEnumerable<FoFinanceLedgerDto>>
{
    private readonly IRepository<FoFinanceLedger> _repo;
    private readonly ITenantContext _tenantContext;

    public GetFoFinanceLedgerQueryHandler(IRepository<FoFinanceLedger> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<FoFinanceLedgerDto>> Handle(
        GetFoFinanceLedgerQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var rows = await _repo.FindAsync(
            f => f.TenantId == tenantId &&
                 (request.TradeDate == null || f.TradeDate == request.TradeDate) &&
                 (request.Exchange == null || f.Exchange == request.Exchange) &&
                 (request.ClientCode == null || f.ClientCode == request.ClientCode),
            cancellationToken);

        return rows
            .OrderBy(f => f.TradeDate)
            .ThenBy(f => f.ClientCode)
            .Select(ToDto);
    }

    internal static FoFinanceLedgerDto ToDto(FoFinanceLedger f) => new(
        f.Id, f.TenantId,
        f.TradeDate, f.Exchange, f.ClearingMemberId, f.BrokerId,
        f.ClientCode, f.ClientId, f.ClientName,
        f.BuyTurnover, f.SellTurnover, f.TotalTurnover,
        f.TotalStt, f.TotalStampDuty,
        f.Brokerage, f.ExchangeTransactionCharges, f.SebiCharges, f.Ipft, f.GstOnCharges,
        f.TotalCharges,
        f.DailyMtmSettlement, f.NetPremium, f.FinalSettlement, f.ExerciseAssignmentValue,
        f.NetAmount);
}
