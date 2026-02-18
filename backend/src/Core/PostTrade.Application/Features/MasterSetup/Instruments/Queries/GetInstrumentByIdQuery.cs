using MediatR;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Instruments.Queries;

public record GetInstrumentByIdQuery(Guid InstrumentId) : IRequest<InstrumentDto?>;

public class GetInstrumentByIdQueryHandler : IRequestHandler<GetInstrumentByIdQuery, InstrumentDto?>
{
    private readonly IRepository<Instrument> _repo;
    private readonly ITenantContext _tenantContext;

    public GetInstrumentByIdQueryHandler(IRepository<Instrument> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<InstrumentDto?> Handle(GetInstrumentByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(i => i.InstrumentId == request.InstrumentId && i.TenantId == tenantId, cancellationToken);
        var i = results.FirstOrDefault();
        if (i is null) return null;
        return new InstrumentDto(i.InstrumentId, i.TenantId, i.InstrumentCode, i.InstrumentName, i.Symbol, i.ISIN,
            i.ExchangeId, i.SegmentId, i.InstrumentType, i.LotSize, i.TickSize,
            i.Series, i.ExpiryDate, i.StrikePrice, i.OptionType, i.Status);
    }
}
