using FluentValidation;
using MediatR;
using PostTrade.Application.Features.EOD.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Application.Features.EOD.Commands;

public record RunEodCommand(DateTime TradingDate) : IRequest<EodRunResultDto>;

public class RunEodCommandValidator : AbstractValidator<RunEodCommand>
{
    public RunEodCommandValidator()
    {
        RuleFor(x => x.TradingDate)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
            .WithMessage("TradingDate cannot be in the future.");
    }
}

public class RunEodCommandHandler : IRequestHandler<RunEodCommand, EodRunResultDto>
{
    private readonly IRepository<Position> _positionRepo;
    private readonly IRepository<PnLSnapshot> _snapshotRepo;
    private readonly IEODProcessingService _eodService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RunEodCommandHandler(
        IRepository<Position> positionRepo,
        IRepository<PnLSnapshot> snapshotRepo,
        IEODProcessingService eodService,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _positionRepo = positionRepo;
        _snapshotRepo = snapshotRepo;
        _eodService = eodService;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<EodRunResultDto> Handle(RunEodCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var tradingDate = request.TradingDate.Date;

        // Guard: prevent re-running EOD for the same date
        var existingSnapshots = await _snapshotRepo.FindAsync(
            s => s.TenantId == tenantId && s.SnapshotDate.Date == tradingDate,
            cancellationToken);

        if (existingSnapshots.Any())
            throw new InvalidOperationException($"EOD has already been processed for {tradingDate:yyyy-MM-dd}.");

        // 1. Fetch all open positions for this tenant (NetQuantity != 0)
        var openPositions = await _positionRepo.FindAsync(
            p => p.TenantId == tenantId && p.NetQuantity != 0,
            cancellationToken);

        var positionList = openPositions.ToList();

        // 2. Snapshot each open position → PnLSnapshot
        var snapshotTime = DateTime.UtcNow;
        foreach (var position in positionList)
        {
            var snapshot = new PnLSnapshot
            {
                PnLId = Guid.NewGuid(),
                TenantId = tenantId,
                BrokerId = position.BrokerId,
                ClientId = position.ClientId,
                InstrumentId = position.InstrumentId,
                SnapshotDate = tradingDate,
                SnapshotTime = snapshotTime,
                RealizedPnL = position.RealizedPnL,
                UnrealizedPnL = position.UnrealizedPnL,
                TotalPnL = position.TotalPnL,
                Brokerage = 0,
                Taxes = 0,
                NetPnL = position.TotalPnL,
                OpenQuantity = position.NetQuantity,
                AveragePrice = position.AverageBuyPrice,
                MarketPrice = position.MarketPrice
            };

            await _snapshotRepo.AddAsync(snapshot, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Run the infrastructure-level EOD orchestration (logging, downstream steps)
        var success = await _eodService.ProcessEndOfDayAsync(tradingDate);

        return new EodRunResultDto(
            tradingDate,
            success,
            positionList.Count,
            success
                ? $"EOD completed successfully. {positionList.Count} position(s) snapshotted."
                : "EOD orchestration reported a failure. Check application logs.");
    }
}
