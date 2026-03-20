using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

public record GetFoContractMastersQuery(
    string? Exchange,
    DateOnly? TradingDate,
    string? Symbol,
    string? ContractType,   // FinInstrmTp: IDF, STF, IDO, STO
    string? OptionType,     // OptnTp: CE, PE
    int Page = 1,
    int PageSize = 50
) : IRequest<IEnumerable<FoContractMasterDto>>;

public class GetFoContractMastersQueryHandler : IRequestHandler<GetFoContractMastersQuery, IEnumerable<FoContractMasterDto>>
{
    private readonly IRepository<FoContractMaster> _contractRepo;
    private readonly ITenantContext _tenantContext;

    public GetFoContractMastersQueryHandler(IRepository<FoContractMaster> contractRepo, ITenantContext tenantContext)
    {
        _contractRepo = contractRepo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<FoContractMasterDto>> Handle(GetFoContractMastersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var all = await _contractRepo.FindAsync(c => c.TenantId == tenantId, cancellationToken);
        var query = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Exchange))
            query = query.Where(c => c.Exchange == request.Exchange);
        if (request.TradingDate.HasValue)
            query = query.Where(c => c.TradingDate == request.TradingDate.Value);
        if (!string.IsNullOrWhiteSpace(request.Symbol))
            query = query.Where(c => c.TckrSymb.Contains(request.Symbol, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(request.ContractType))
            query = query.Where(c => c.FinInstrmTp == request.ContractType);
        if (!string.IsNullOrWhiteSpace(request.OptionType))
            query = query.Where(c => c.OptnTp == request.OptionType);

        return query
            .OrderBy(c => c.TckrSymb)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new FoContractMasterDto(
                c.ContractRowId, c.TradingDate, c.Exchange, c.FinInstrmId,
                c.UndrlygFinInstrmId, c.TckrSymb, c.FinInstrmNm, c.XpryDt, c.ExpiryDate,
                c.StrkPric, c.OptnTp, c.FinInstrmTp, c.OptnExrcStyle, c.SttlmMtd,
                c.MinLot, c.NewBrdLotQty, c.TickSize, c.BasePric, c.MktTpAndId,
                c.Isin, c.RegisteredInstrumentId))
            .ToList();
    }
}
