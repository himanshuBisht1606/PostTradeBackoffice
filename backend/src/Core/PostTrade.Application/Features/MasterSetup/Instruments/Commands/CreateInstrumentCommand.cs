using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Instruments.Commands;

public record CreateInstrumentCommand(
    string InstrumentCode,
    string InstrumentName,
    string Symbol,
    string? ISIN,
    Guid ExchangeId,
    Guid SegmentId,
    InstrumentType InstrumentType,
    decimal LotSize,
    decimal TickSize,
    string? Series,
    DateTime? ExpiryDate,
    decimal? StrikePrice,
    OptionType? OptionType
) : IRequest<InstrumentDto>;

public class CreateInstrumentCommandValidator : AbstractValidator<CreateInstrumentCommand>
{
    public CreateInstrumentCommandValidator()
    {
        RuleFor(x => x.InstrumentCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InstrumentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ExchangeId).NotEmpty();
        RuleFor(x => x.SegmentId).NotEmpty();
        RuleFor(x => x.LotSize).GreaterThan(0);
        RuleFor(x => x.TickSize).GreaterThan(0);
    }
}

public class CreateInstrumentCommandHandler : IRequestHandler<CreateInstrumentCommand, InstrumentDto>
{
    private readonly IRepository<Instrument> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateInstrumentCommandHandler(IRepository<Instrument> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<InstrumentDto> Handle(CreateInstrumentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var instrument = new Instrument
        {
            InstrumentId = Guid.NewGuid(),
            TenantId = tenantId,
            InstrumentCode = request.InstrumentCode,
            InstrumentName = request.InstrumentName,
            Symbol = request.Symbol,
            ISIN = request.ISIN,
            ExchangeId = request.ExchangeId,
            SegmentId = request.SegmentId,
            InstrumentType = request.InstrumentType,
            LotSize = request.LotSize,
            TickSize = request.TickSize,
            Series = request.Series,
            ExpiryDate = request.ExpiryDate,
            StrikePrice = request.StrikePrice,
            OptionType = request.OptionType,
            Status = InstrumentStatus.Active
        };

        await _repo.AddAsync(instrument, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InstrumentDto(instrument.InstrumentId, instrument.TenantId, instrument.InstrumentCode,
            instrument.InstrumentName, instrument.Symbol, instrument.ISIN, instrument.ExchangeId,
            instrument.SegmentId, instrument.InstrumentType, instrument.LotSize, instrument.TickSize,
            instrument.Series, instrument.ExpiryDate, instrument.StrikePrice, instrument.OptionType, instrument.Status);
    }
}
