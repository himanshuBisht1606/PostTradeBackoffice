using PostTrade.Application.Features.Settlement.Batches.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Settlement;

public class GetSettlementBatchesQueryHandlerTests
{
    private readonly Mock<IRepository<SettlementBatch>> _repo          = new();
    private readonly Mock<ITenantContext>               _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetSettlementBatchesQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetSettlementBatchesQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static SettlementBatch MakeBatch(SettlementStatus status = SettlementStatus.Pending) => new()
    {
        BatchId        = Guid.NewGuid(),
        TenantId       = TenantId,
        SettlementNo   = "SN" + Guid.NewGuid().ToString("N")[..8],
        TradeDate      = DateTime.Today,
        SettlementDate = DateTime.Today.AddDays(2),
        ExchangeId     = Guid.NewGuid(),
        TotalTrades    = 5,
        TotalTurnover  = 250000m,
        Status         = status
    };

    [Fact]
    public async Task Handle_ShouldReturnAllBatchesForTenant()
    {
        var batches = new List<SettlementBatch> { MakeBatch(), MakeBatch() };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(batches);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetSettlementBatchesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoBatches_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementBatch>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetSettlementBatchesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByStatus_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementBatch>());

        var handler = CreateHandler();
        await handler.Handle(new GetSettlementBatchesQuery(SettlementStatus.Completed), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementBatch>());

        var handler = CreateHandler();
        await handler.Handle(new GetSettlementBatchesQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
