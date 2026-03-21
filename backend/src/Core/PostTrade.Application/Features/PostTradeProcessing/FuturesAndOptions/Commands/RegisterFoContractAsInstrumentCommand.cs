using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record RegisterFoContractAsInstrumentCommand(Guid ContractRowId) : IRequest<InstrumentDto>;

public class RegisterFoContractAsInstrumentCommandValidator : AbstractValidator<RegisterFoContractAsInstrumentCommand>
{
    public RegisterFoContractAsInstrumentCommandValidator()
    {
        RuleFor(x => x.ContractRowId).NotEmpty();
    }
}

public class RegisterFoContractAsInstrumentCommandHandler
    : IRequestHandler<RegisterFoContractAsInstrumentCommand, InstrumentDto>
{
    private readonly IRepository<FoContractMaster> _contractRepo;
    private readonly IRepository<Instrument> _instrumentRepo;
    private readonly IRepository<ExchangeSegment> _exchangeSegmentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RegisterFoContractAsInstrumentCommandHandler(
        IRepository<FoContractMaster> contractRepo,
        IRepository<Instrument> instrumentRepo,
        IRepository<ExchangeSegment> exchangeSegmentRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _contractRepo = contractRepo;
        _instrumentRepo = instrumentRepo;
        _exchangeSegmentRepo = exchangeSegmentRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<InstrumentDto> Handle(RegisterFoContractAsInstrumentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var contract = await _contractRepo.FirstOrDefaultAsync(
            c => c.ContractRowId == request.ContractRowId && c.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"FO contract {request.ContractRowId} not found.");

        if (contract.RegisteredInstrumentId.HasValue)
            throw new InvalidOperationException("This contract is already registered as an instrument.");

        // Map exchange code to ExchangeSegment code
        var exchangeSegmentCode = contract.Exchange switch
        {
            "NFO" => "NSE-FO",
            "BFO" => "BSE-FO",
            _ => throw new InvalidOperationException($"Unsupported exchange: {contract.Exchange}")
        };

        var exchangeSegment = await _exchangeSegmentRepo.FirstOrDefaultAsync(
            es => es.TenantId == tenantId && es.ExchangeSegmentCode == exchangeSegmentCode, cancellationToken)
            ?? throw new InvalidOperationException($"Exchange segment '{exchangeSegmentCode}' not found. Ensure it is seeded.");

        // Derive instrument type from FinInstrmTp
        var instrumentType = contract.FinInstrmTp switch
        {
            "IDO" or "STO" => InstrumentType.Option,
            "IDF" or "STF" => InstrumentType.Future,
            _ => InstrumentType.Derivative
        };

        // Derive option type from OptnTp
        OptionType? optionType = contract.OptnTp switch
        {
            "CE" => OptionType.Call,
            "PE" => OptionType.Put,
            _ => null
        };

        var instrument = new Instrument
        {
            InstrumentId = Guid.NewGuid(),
            TenantId = tenantId,
            InstrumentCode = $"{contract.TckrSymb}",
            InstrumentName = !string.IsNullOrWhiteSpace(contract.FinInstrmNm)
                ? contract.FinInstrmNm
                : contract.StockNm,
            Symbol = contract.TckrSymb,
            ExchangeId = exchangeSegment.ExchangeId,
            SegmentId = exchangeSegment.SegmentId,
            InstrumentType = instrumentType,
            LotSize = contract.NewBrdLotQty > 0 ? contract.NewBrdLotQty : contract.MinLot,
            TickSize = 0.05m,
            ExpiryDate = contract.ExpiryDate.HasValue
                ? contract.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue)
                : null,
            StrikePrice = contract.StrkPric > 0 ? contract.StrkPric : null,
            OptionType = optionType,
            Status = InstrumentStatus.Active
        };

        await _instrumentRepo.AddAsync(instrument, cancellationToken);

        // Link contract back to the registered instrument
        contract.RegisteredInstrumentId = instrument.InstrumentId;
        await _contractRepo.UpdateAsync(contract, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InstrumentDto(
            instrument.InstrumentId, instrument.TenantId, instrument.InstrumentCode,
            instrument.InstrumentName, instrument.Symbol, instrument.ISIN,
            instrument.ExchangeId, instrument.SegmentId, instrument.InstrumentType,
            instrument.LotSize, instrument.TickSize, instrument.Series,
            instrument.ExpiryDate, instrument.StrikePrice, instrument.OptionType, instrument.Status);
    }
}
