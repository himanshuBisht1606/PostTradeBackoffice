using PostTrade.Application.Features.Trading.Trades.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Trading.Trades;

public class GetTradeByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Trade>> _repo          = new();
    private readonly Mock<ITenantContext>     _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetTradeByIdQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetTradeByIdQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private Trade ExistingTrade(Guid tradeId) => new()
    {
        TradeId      = tradeId,
        TenantId     = TenantId,
        BrokerId     = Guid.NewGuid(),
        ClientId     = Guid.NewGuid(),
        InstrumentId = Guid.NewGuid(),
        TradeNo      = "TRD20240101000000001",
        Side         = TradeSide.Buy,
        Quantity     = 100,
        Price        = 250m,
        TradeValue   = 25000m,
        TradeDate    = DateTime.Today,
        TradeTime    = DateTime.UtcNow,
        SettlementNo = "SN001",
        Status       = TradeStatus.Pending,
        Source       = TradeSource.Manual,
        NetAmount    = 25000m
    };

    [Fact]
    public async Task Handle_WhenTradeExists_ShouldReturnDto()
    {
        var tradeId = Guid.NewGuid();
        var trade   = ExistingTrade(tradeId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradeByIdQuery(tradeId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TradeId.Should().Be(tradeId);
        result.TradeNo.Should().Be(trade.TradeNo);
        result.Status.Should().Be(trade.Status);
    }

    [Fact]
    public async Task Handle_WhenTradeNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        await handler.Handle(new GetTradeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
