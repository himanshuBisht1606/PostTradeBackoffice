using PostTrade.Application.Features.Trading.Trades.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Trading.Trades;

public class GetTradesQueryHandlerTests
{
    private readonly Mock<IRepository<Trade>> _repo          = new();
    private readonly Mock<ITenantContext>     _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetTradesQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetTradesQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Trade MakeTrade(int index = 1, TradeStatus status = TradeStatus.Pending) => new()
    {
        TradeId      = Guid.NewGuid(),
        TenantId     = TenantId,
        BrokerId     = Guid.NewGuid(),
        ClientId     = ClientId,
        InstrumentId = InstrumentId,
        TradeNo      = $"TRD{index:D20}",
        Side         = TradeSide.Buy,
        Quantity     = 100,
        Price        = 250m,
        TradeValue   = 25000m,
        TradeDate    = DateTime.Today,
        TradeTime    = DateTime.UtcNow,
        SettlementNo = $"SN{index:D3}",
        Status       = status,
        Source       = TradeSource.Manual,
        NetAmount    = 25000m
    };

    [Fact]
    public async Task Handle_ShouldReturnAllTradesForTenant()
    {
        var trades = new List<Trade> { MakeTrade(1), MakeTrade(2), MakeTrade(3) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(trades);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenNoTrades_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByClientId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        await handler.Handle(new GetTradesQuery(ClientId: ClientId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteringByDateRange_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        await handler.Handle(new GetTradesQuery(FromDate: DateTime.Today.AddDays(-7), ToDate: DateTime.Today), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteringByStatus_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        await handler.Handle(new GetTradesQuery(Status: TradeStatus.Settled), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteringByInstrumentId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        await handler.Handle(new GetTradesQuery(InstrumentId: InstrumentId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var trade = MakeTrade(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetTradesQuery(), CancellationToken.None)).First();

        result.TradeId.Should().Be(trade.TradeId);
        result.TenantId.Should().Be(trade.TenantId);
        result.ClientId.Should().Be(trade.ClientId);
        result.InstrumentId.Should().Be(trade.InstrumentId);
        result.TradeNo.Should().Be(trade.TradeNo);
        result.Side.Should().Be(trade.Side);
        result.Quantity.Should().Be(trade.Quantity);
        result.Price.Should().Be(trade.Price);
        result.TradeValue.Should().Be(trade.TradeValue);
        result.Status.Should().Be(trade.Status);
    }
}
