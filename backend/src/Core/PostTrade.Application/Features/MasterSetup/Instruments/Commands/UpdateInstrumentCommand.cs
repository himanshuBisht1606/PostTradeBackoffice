using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Instruments.Commands;

public record UpdateInstrumentCommand(
    Guid InstrumentId,
    string InstrumentName,
    decimal LotSize,
    decimal TickSize,
    InstrumentStatus Status,
    DateTime? ExpiryDate,
    decimal? StrikePrice
) : IRequest<InstrumentDto?>;

public class UpdateInstrumentCommandValidator : AbstractValidator<UpdateInstrumentCommand>
{
    public UpdateInstrumentCommandValidator()
    {
        RuleFor(x => x.InstrumentId).NotEmpty();
        RuleFor(x => x.InstrumentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LotSize).GreaterThan(0);
        RuleFor(x => x.TickSize).GreaterThan(0);
    }
}

public class UpdateInstrumentCommandHandler : IRequestHandler<UpdateInstrumentCommand, InstrumentDto?>
{
    private readonly IRepository<Instrument> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateInstrumentCommandHandler(IRepository<Instrument> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<InstrumentDto?> Handle(UpdateInstrumentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(i => i.InstrumentId == request.InstrumentId && i.TenantId == tenantId, cancellationToken);
        var instrument = results.FirstOrDefault();
        if (instrument is null) return null;

        instrument.InstrumentName = request.InstrumentName;
        instrument.LotSize = request.LotSize;
        instrument.TickSize = request.TickSize;
        instrument.Status = request.Status;
        instrument.ExpiryDate = request.ExpiryDate;
        instrument.StrikePrice = request.StrikePrice;

        await _repo.UpdateAsync(instrument, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InstrumentDto(instrument.InstrumentId, instrument.TenantId, instrument.InstrumentCode,
            instrument.InstrumentName, instrument.Symbol, instrument.ISIN, instrument.ExchangeId,
            instrument.SegmentId, instrument.InstrumentType, instrument.LotSize, instrument.TickSize,
            instrument.Series, instrument.ExpiryDate, instrument.StrikePrice, instrument.OptionType, instrument.Status);
    }
}
