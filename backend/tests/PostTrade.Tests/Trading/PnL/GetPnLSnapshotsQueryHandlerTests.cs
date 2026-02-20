using PostTrade.Application.Features.Trading.PnL.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Tests.Trading.PnL;

public class GetPnLSnapshotsQueryHandlerTests
{
    private readonly Mock<IRepository<PnLSnapshot>> _repo          = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetPnLSnapshotsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetPnLSnapshotsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static PnLSnapshot MakeSnapshot(int dayOffset = 0) => new()
    {
        PnLId        = Guid.NewGuid(),
        TenantId     = TenantId,
        BrokerId     = Guid.NewGuid(),
        ClientId     = ClientId,
        InstrumentId = InstrumentId,
        SnapshotDate = DateTime.Today.AddDays(-dayOffset),
        SnapshotTime = DateTime.UtcNow.AddDays(-dayOffset),
        RealizedPnL  = 1000m,
        TotalPnL     = 1500m,
        NetPnL       = 1450m,
        OpenQuantity = 50,
        AveragePrice = 245m,
        MarketPrice  = 260m
    };

    [Fact]
    public async Task Handle_ShouldReturnAllSnapshotsForTenant()
    {
        var snapshots = new List<PnLSnapshot> { MakeSnapshot(0), MakeSnapshot(1) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(snapshots);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPnLSnapshotsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoSnapshots_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPnLSnapshotsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnSnapshotsOrderedByDateDescending()
    {
        var older  = MakeSnapshot(2);
        var newer  = MakeSnapshot(0);
        var middle = MakeSnapshot(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot> { older, newer, middle });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetPnLSnapshotsQuery(), CancellationToken.None)).ToList();

        result[0].SnapshotDate.Should().BeOnOrAfter(result[1].SnapshotDate);
        result[1].SnapshotDate.Should().BeOnOrAfter(result[2].SnapshotDate);
    }

    [Fact]
    public async Task Handle_WhenFilteringByClientId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        await handler.Handle(new GetPnLSnapshotsQuery(ClientId: ClientId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        await handler.Handle(new GetPnLSnapshotsQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
