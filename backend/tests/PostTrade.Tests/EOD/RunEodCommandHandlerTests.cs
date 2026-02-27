using PostTrade.Application.Features.EOD.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Tests.EOD;

public class RunEodCommandHandlerTests
{
    private readonly Mock<IRepository<Position>>   _positionRepo  = new();
    private readonly Mock<IRepository<PnLSnapshot>> _snapshotRepo  = new();
    private readonly Mock<IEODProcessingService>   _eodService    = new();
    private readonly Mock<IUnitOfWork>             _unitOfWork    = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private RunEodCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _eodService.Setup(s => s.ProcessEndOfDayAsync(It.IsAny<DateTime>())).ReturnsAsync(true);
        _snapshotRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PnLSnapshot>()); // no existing snapshots
        _positionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Position>());
        _snapshotRepo.Setup(r => r.AddAsync(It.IsAny<PnLSnapshot>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((PnLSnapshot s, CancellationToken _) => s);
        return new RunEodCommandHandler(
            _positionRepo.Object, _snapshotRepo.Object, _eodService.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static Position MakeOpenPosition() => new()
    {
        PositionId      = Guid.NewGuid(),
        TenantId        = TenantId,
        BrokerId        = Guid.NewGuid(),
        ClientId        = Guid.NewGuid(),
        InstrumentId    = Guid.NewGuid(),
        PositionDate    = DateTime.Today,
        NetQuantity     = 100,
        BuyQuantity     = 100,
        AverageBuyPrice = 250m,
        MarketPrice     = 260m,
        RealizedPnL     = 0m,
        UnrealizedPnL   = 1000m,
        TotalPnL        = 1000m
    };

    [Fact]
    public async Task Handle_WhenAlreadyProcessedForDate_ShouldThrowInvalidOperationException()
    {
        var handler = CreateHandler();

        // Override: existing snapshots present → already processed
        _snapshotRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PnLSnapshot> { new() { PnLId = Guid.NewGuid() } });

        var act = () => handler.Handle(new RunEodCommand(DateTime.Today), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldCreateSnapshotForEachOpenPosition()
    {
        var handler = CreateHandler();

        _positionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Position> { MakeOpenPosition(), MakeOpenPosition() });

        await handler.Handle(new RunEodCommand(DateTime.Today), CancellationToken.None);

        _snapshotRepo.Verify(r => r.AddAsync(It.IsAny<PnLSnapshot>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenNoOpenPositions_ShouldReturnZeroSnapshotted()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(new RunEodCommand(DateTime.Today), CancellationToken.None);

        result.PositionsSnapshotted.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCallEodProcessingService()
    {
        var handler = CreateHandler();
        await handler.Handle(new RunEodCommand(DateTime.Today), CancellationToken.None);

        _eodService.Verify(s => s.ProcessEndOfDayAsync(It.IsAny<DateTime>()), Times.Once);
    }
}
