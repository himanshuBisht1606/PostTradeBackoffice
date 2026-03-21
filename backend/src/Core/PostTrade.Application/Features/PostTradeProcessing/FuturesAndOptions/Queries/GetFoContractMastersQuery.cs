using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

// ── Raw exchange fields (admin/debug view) ────────────────────────────────────
public record GetFoContractMastersQuery(
    string? Exchange,
    DateOnly? TradingDate,
    string? Symbol,
    string? ContractType,   // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    string? OptionType,     // CE | PE
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
            .ThenBy(c => c.ExpiryDate)
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

// ── Curated FoContracts table (Master Setup → FO Instruments view) ───────────

public record GetFoContractsQuery(
    string? Exchange,
    DateOnly? TradingDate,
    string? Symbol,
    string? InstrumentType,   // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    string? OptionType,       // CE | PE | FX
    int Page = 1,
    int PageSize = 50
) : IRequest<IEnumerable<FoContractDto>>;

public class GetFoContractsQueryHandler : IRequestHandler<GetFoContractsQuery, IEnumerable<FoContractDto>>
{
    private readonly IRepository<FoContract> _repo;
    private readonly ITenantContext _tenantContext;

    public GetFoContractsQueryHandler(IRepository<FoContract> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<FoContractDto>> Handle(GetFoContractsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var all = await _repo.FindAsync(c => c.TenantId == tenantId, cancellationToken);
        var query = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Exchange))
            query = query.Where(c => c.Exchange == request.Exchange);
        if (request.TradingDate.HasValue)
            query = query.Where(c => c.TradingDate == request.TradingDate.Value);
        if (!string.IsNullOrWhiteSpace(request.Symbol))
            query = query.Where(c => c.Symbol.Contains(request.Symbol, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(request.InstrumentType))
            query = query.Where(c => c.InstrumentType == request.InstrumentType);
        if (!string.IsNullOrWhiteSpace(request.OptionType))
            query = query.Where(c => c.OptionType == request.OptionType);

        return query
            .OrderBy(c => c.InstrumentType)
            .ThenBy(c => c.Symbol)
            .ThenBy(c => c.ExpiryDate)
            .ThenBy(c => c.StrikePrice)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new FoContractDto(
                c.ContractId, c.Exchange, c.TradingDate,
                c.InstrumentType, c.Symbol, c.ContractName,
                c.ExpiryDate, c.StrikePrice, c.OptionType,
                c.LotSize, c.FMultiplier, c.FinInstrmId,
                c.UnderlyingSymbol, c.Isin, c.TickSize, c.SttlmMtd,
                c.RegisteredInstrumentId))
            .ToList();
    }
}

// ── Broker contract book view (matches cONTRACT.xls format) ──────────────────
public record GetFoContractBookQuery(
    string? Exchange,
    DateOnly? TradingDate,
    string? Symbol,
    string? ContractType,   // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    string? OptionType,     // CE | PE
    int Page = 1,
    int PageSize = 100
) : IRequest<FoContractBookPagedDto>;

public class GetFoContractBookQueryHandler : IRequestHandler<GetFoContractBookQuery, FoContractBookPagedDto>
{
    private readonly IRepository<FoContractMaster> _contractRepo;
    private readonly ITenantContext _tenantContext;

    public GetFoContractBookQueryHandler(IRepository<FoContractMaster> contractRepo, ITenantContext tenantContext)
    {
        _contractRepo = contractRepo;
        _tenantContext = tenantContext;
    }

    public async Task<FoContractBookPagedDto> Handle(GetFoContractBookQuery request, CancellationToken cancellationToken)
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

        var sorted = query
            .OrderBy(c => c.FinInstrmTp)
            .ThenBy(c => c.TckrSymb)
            .ThenBy(c => c.ExpiryDate)
            .ThenBy(c => c.StrkPric)
            .ToList();

        var total = sorted.Count;
        var items = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToContractBookItem);

        return new FoContractBookPagedDto(items, total, request.Page, request.PageSize);
    }

    private static FoContractBookItemDto ToContractBookItem(FoContractMaster c) => new(
        InstrumentType:       c.FinInstrmTp,
        Symbol:               c.TckrSymb,
        LotSize:              c.NewBrdLotQty > 0 ? c.NewBrdLotQty : c.MinLot,
        ExpiryDate:           c.ExpiryDate,
        ContName:             BuildContName(c.FinInstrmTp, c.TckrSymb, c.ExpiryDate),
        Exchange:             c.Exchange,
        CMultiplier:          1,
        Strike:               c.StrkPric > 0 ? Math.Round(c.StrkPric / 100m, 2) : 0m,
        OptionType:           c.OptnTp,
        InteropSymbol:        null,   // not in exchange files; populated from client config if needed
        TickSize:             c.TickSize,
        OptionExerciseStyle:  c.OptnExrcStyle,
        Segment:              "FO",
        Isin:                 c.Isin,
        ContractRowId:        c.ContractRowId
    );

    /// <summary>
    /// Builds the broker-style contract name matching cONTRACT.xls CONTNAME column.
    /// Format: InstrumentType + Symbol + ExpiryDate(DDMMMYYYY uppercase)
    /// Example: OPTSTKINFY28APR2026, FUTIDXNIFTY27MAR2026
    /// </summary>
    internal static string BuildContName(string instrTp, string symbol, DateOnly? expiry)
    {
        if (expiry == null) return $"{instrTp}{symbol}";
        // Format: 28APR2026
        var expiryPart = expiry.Value.ToString("ddMMMyyyy").ToUpperInvariant();
        return $"{instrTp}{symbol}{expiryPart}";
    }
}
