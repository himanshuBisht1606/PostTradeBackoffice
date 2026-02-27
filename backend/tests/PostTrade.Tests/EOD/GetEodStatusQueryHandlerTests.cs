using PostTrade.Application.Features.EOD.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Tests.EOD;

public class GetEodStatusQueryHandlerTests
{
    private readonly Mock<IRepository<PnLSnapshot>> _snapshotRepo  = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetEodStatusQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetEodStatusQueryHandler(_snapshotRepo.Object, _tenantContext.Object);
    }

    private static PnLSnapshot MakeSnapshot(DateTime? snapshotTime = null) => new()
    {
        PnLId        = Guid.NewGuid(),
        TenantId     = TenantId,
        BrokerId     = Guid.NewGuid(),
        ClientId     = Guid.NewGuid(),
        InstrumentId = Guid.NewGuid(),
        SnapshotDate = DateTime.Today,
        SnapshotTime = snapshotTime ?? DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_WhenSnapshotsExist_ShouldReturnIsProcessedTrue()
    {
        _snapshotRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PnLSnapshot> { MakeSnapshot(), MakeSnapshot() });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetEodStatusQuery(DateTime.Today), CancellationToken.None);

        result.IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoSnapshots_ShouldReturnIsProcessedFalse()
    {
        _snapshotRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetEodStatusQuery(DateTime.Today), CancellationToken.None);

        result.IsProcessed.Should().BeFalse();
        result.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectSnapshotCount()
    {
        _snapshotRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PnLSnapshot> { MakeSnapshot(), MakeSnapshot(), MakeSnapshot() });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetEodStatusQuery(DateTime.Today), CancellationToken.None);

        result.SnapshotCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _snapshotRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        await handler.Handle(new GetEodStatusQuery(DateTime.Today), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
