using PostTrade.Application.Features.Settlement.Obligations.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Settlement;

public class GetSettlementObligationsQueryHandlerTests
{
    private readonly Mock<IRepository<SettlementObligation>> _repo          = new();
    private readonly Mock<ITenantContext>                    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid BatchId  = Guid.NewGuid();

    private GetSettlementObligationsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetSettlementObligationsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static SettlementObligation MakeObligation(ObligationStatus status = ObligationStatus.Pending) => new()
    {
        ObligationId            = Guid.NewGuid(),
        TenantId                = TenantId,
        BrokerId                = Guid.NewGuid(),
        ClientId                = Guid.NewGuid(),
        BatchId                 = BatchId,
        SettlementNo            = "SN20240101",
        Status                  = status,
        FundsPayIn              = 10000m,
        FundsPayOut             = 0m,
        NetFundsObligation      = 10000m,
        SecuritiesPayIn         = 100,
        SecuritiesPayOut        = 0,
        NetSecuritiesObligation = 100
    };

    [Fact]
    public async Task Handle_ShouldReturnAllObligationsForTenant()
    {
        var obligations = new List<SettlementObligation> { MakeObligation(), MakeObligation() };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(obligations);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetSettlementObligationsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoObligations_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetSettlementObligationsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByBatchId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation>());

        var handler = CreateHandler();
        await handler.Handle(new GetSettlementObligationsQuery(BatchId: BatchId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation>());

        var handler = CreateHandler();
        await handler.Handle(new GetSettlementObligationsQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
