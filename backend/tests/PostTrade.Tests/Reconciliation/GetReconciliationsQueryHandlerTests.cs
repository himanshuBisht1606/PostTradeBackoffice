using PostTrade.Application.Features.Reconciliation.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Enums;
using ReconEntity = PostTrade.Domain.Entities.Reconciliation.Reconciliation;

namespace PostTrade.Tests.Reconciliation;

public class GetReconciliationsQueryHandlerTests
{
    private readonly Mock<IRepository<ReconEntity>> _repo          = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetReconciliationsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetReconciliationsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static ReconEntity MakeRecon(ReconStatus status = ReconStatus.Matched) => new()
    {
        ReconId        = Guid.NewGuid(),
        TenantId       = TenantId,
        ReconDate      = DateTime.Today,
        SettlementNo   = "SN001",
        ReconType      = ReconType.Trade,
        SystemValue    = 1000m,
        ExchangeValue  = 1000m,
        Difference     = 0m,
        ToleranceLimit = 5m,
        Status         = status
    };

    [Fact]
    public async Task Handle_ShouldReturnAllReconciliationsForTenant()
    {
        var records = new List<ReconEntity> { MakeRecon(), MakeRecon() };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconEntity, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(records);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetReconciliationsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoRecords_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconEntity, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconEntity>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetReconciliationsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByStatus_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconEntity, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconEntity>());

        var handler = CreateHandler();
        await handler.Handle(new GetReconciliationsQuery(Status: ReconStatus.Mismatched), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ReconEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconEntity, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconEntity>());

        var handler = CreateHandler();
        await handler.Handle(new GetReconciliationsQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
