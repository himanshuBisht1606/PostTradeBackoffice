using MediatR;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

/// <summary>
/// Registers a curated FoContract (from FoContracts table) as a master Instrument.
/// Uses the already-normalized fields (InstrumentType, StrikePrice in ₹, OptionType CE/PE/FX).
/// </summary>
public record RegisterFoCuratedContractCommand(Guid ContractId) : IRequest<InstrumentDto>;

public class RegisterFoCuratedContractCommandHandler
    : IRequestHandler<RegisterFoCuratedContractCommand, InstrumentDto>
{
    private readonly IRepository<FoContract> _contractRepo;
    private readonly IRepository<Instrument> _instrumentRepo;
    private readonly IRepository<ExchangeSegment> _exchangeSegmentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RegisterFoCuratedContractCommandHandler(
        IRepository<FoContract> contractRepo,
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

    public async Task<InstrumentDto> Handle(RegisterFoCuratedContractCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var contract = await _contractRepo.FirstOrDefaultAsync(
            c => c.ContractId == request.ContractId && c.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"FO contract {request.ContractId} not found.");

        if (contract.RegisteredInstrumentId.HasValue)
            throw new InvalidOperationException("This contract is already registered as an instrument.");

        // Map exchange to segment code (NFO → NSE-FO, BFO → BSE-FO)
        var exchangeSegmentCode = contract.Exchange switch
        {
            "NFO" => "NSE-FO",
            "BFO" => "BSE-FO",
            _ => throw new InvalidOperationException($"Unsupported exchange: {contract.Exchange}")
        };

        var exchangeSegment = await _exchangeSegmentRepo.FirstOrDefaultAsync(
            es => es.TenantId == tenantId && es.ExchangeSegmentCode == exchangeSegmentCode, cancellationToken)
            ?? throw new InvalidOperationException($"Exchange segment '{exchangeSegmentCode}' not configured.");

        // InstrumentType from normalized FUTIDX/FUTSTK/OPTIDX/OPTSTK
        var instrumentType = contract.InstrumentType switch
        {
            "OPTIDX" or "OPTSTK" => InstrumentType.Option,
            "FUTIDX" or "FUTSTK" => InstrumentType.Future,
            _ => InstrumentType.Derivative
        };

        // OptionType from CE/PE/FX — FX (futures) maps to null
        OptionType? optionType = contract.OptionType switch
        {
            "CE" => OptionType.Call,
            "PE" => OptionType.Put,
            _ => null
        };

        var instrument = new Instrument
        {
            InstrumentId    = Guid.NewGuid(),
            TenantId        = tenantId,
            InstrumentCode  = contract.Symbol,
            InstrumentName  = contract.ContractName,   // e.g. FUTIDXNIFTY27MAR2025
            Symbol          = contract.Symbol,
            ISIN            = contract.Isin,
            ExchangeId      = exchangeSegment.ExchangeId,
            SegmentId       = exchangeSegment.SegmentId,
            InstrumentType  = instrumentType,
            LotSize         = contract.LotSize,
            TickSize        = contract.TickSize > 0 ? contract.TickSize : 0.05m,
            ExpiryDate      = contract.ExpiryDate.ToDateTime(TimeOnly.MinValue),
            StrikePrice     = contract.StrikePrice > 0 ? contract.StrikePrice : null,
            OptionType      = optionType,
            Status          = InstrumentStatus.Active
        };

        await _instrumentRepo.AddAsync(instrument, cancellationToken);

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
