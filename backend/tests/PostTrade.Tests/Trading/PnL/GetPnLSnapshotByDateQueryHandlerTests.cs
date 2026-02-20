using PostTrade.Application.Features.Trading.PnL.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Tests.Trading.PnL;

public class GetPnLSnapshotByDateQueryHandlerTests
{
    private readonly Mock<IRepository<PnLSnapshot>> _repo          = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetPnLSnapshotByDateQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetPnLSnapshotByDateQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static PnLSnapshot MakeSnapshot(DateTime date) => new()
    {
        PnLId        = Guid.NewGuid(),
        TenantId     = TenantId,
        BrokerId     = Guid.NewGuid(),
        ClientId     = ClientId,
        InstrumentId = InstrumentId,
        SnapshotDate = date.Date,
        SnapshotTime = date,
        RealizedPnL  = 1000m,
        TotalPnL     = 1500m,
        NetPnL       = 1450m,
        OpenQuantity = 50,
        AveragePrice = 245m,
        MarketPrice  = 260m
    };

    [Fact]
    public async Task Handle_ShouldReturnSnapshotsForDate()
    {
        var date      = DateTime.Today;
        var snapshots = new List<PnLSnapshot> { MakeSnapshot(date), MakeSnapshot(date) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(snapshots);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPnLSnapshotByDateQuery(date), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoSnapshotsForDate_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPnLSnapshotByDateQuery(DateTime.Today), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByClientId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        await handler.Handle(new GetPnLSnapshotByDateQuery(DateTime.Today, ClientId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        await handler.Handle(new GetPnLSnapshotByDateQuery(DateTime.Today), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
