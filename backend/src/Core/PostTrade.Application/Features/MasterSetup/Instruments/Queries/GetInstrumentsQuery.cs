using MediatR;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Instruments.Queries;

public record GetInstrumentsQuery(Guid? ExchangeId = null, InstrumentType? Type = null) : IRequest<IEnumerable<InstrumentDto>>;

public class GetInstrumentsQueryHandler : IRequestHandler<GetInstrumentsQuery, IEnumerable<InstrumentDto>>
{
    private readonly IRepository<Instrument> _repo;
    private readonly ITenantContext _tenantContext;

    public GetInstrumentsQueryHandler(IRepository<Instrument> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<InstrumentDto>> Handle(GetInstrumentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var instruments = await _repo.FindAsync(i =>
            i.TenantId == tenantId &&
            (request.ExchangeId == null || i.ExchangeId == request.ExchangeId) &&
            (request.Type == null || i.InstrumentType == request.Type), cancellationToken);
        return instruments.Select(ToDto);
    }

    private static InstrumentDto ToDto(Instrument i) => new(
        i.InstrumentId, i.TenantId, i.InstrumentCode, i.InstrumentName, i.Symbol, i.ISIN,
        i.ExchangeId, i.SegmentId, i.InstrumentType, i.LotSize, i.TickSize,
        i.Series, i.ExpiryDate, i.StrikePrice, i.OptionType, i.Status);
}
