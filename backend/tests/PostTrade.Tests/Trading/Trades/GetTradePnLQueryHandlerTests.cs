using PostTrade.Application.Features.Trading.Trades.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Trading.Trades;

public class GetTradePnLQueryHandlerTests
{
    private readonly Mock<IRepository<Trade>>       _tradeRepo     = new();
    private readonly Mock<IRepository<PnLSnapshot>> _pnlRepo       = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetTradePnLQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetTradePnLQueryHandler(_tradeRepo.Object, _pnlRepo.Object, _tenantContext.Object);
    }

    private Trade ExistingTrade(Guid tradeId) => new()
    {
        TradeId      = tradeId,
        TenantId     = TenantId,
        ClientId     = ClientId,
        InstrumentId = InstrumentId,
        BrokerId     = Guid.NewGuid(),
        TradeNo      = "TRD20240101000000001",
        Side         = TradeSide.Buy,
        Quantity     = 100,
        Price        = 250m,
        TradeValue   = 25000m,
        TradeDate    = DateTime.Today,
        TradeTime    = DateTime.UtcNow,
        SettlementNo = "SN001",
        Status       = TradeStatus.Settled,
        Source       = TradeSource.Manual,
        NetAmount    = 25000m
    };

    private PnLSnapshot MakeSnapshot(DateTime snapshotTime) => new()
    {
        PnLId         = Guid.NewGuid(),
        TenantId      = TenantId,
        ClientId      = ClientId,
        InstrumentId  = InstrumentId,
        BrokerId      = Guid.NewGuid(),
        SnapshotDate  = snapshotTime.Date,
        SnapshotTime  = snapshotTime,
        RealizedPnL   = 1000m,
        UnrealizedPnL = 500m,
        TotalPnL      = 1500m,
        NetPnL        = 1450m,
        OpenQuantity  = 50,
        AveragePrice  = 245m,
        MarketPrice   = 260m
    };

    [Fact]
    public async Task Handle_WhenTradeNotFound_ShouldReturnNull()
    {
        _tradeRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradePnLQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNoSnapshotFound_ShouldReturnNull()
    {
        var tradeId = Guid.NewGuid();

        _tradeRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Trade> { ExistingTrade(tradeId) });
        _pnlRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PnLSnapshot>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradePnLQuery(tradeId), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnLatestSnapshot()
    {
        var tradeId = Guid.NewGuid();
        var older   = MakeSnapshot(DateTime.UtcNow.AddHours(-2));
        var latest  = MakeSnapshot(DateTime.UtcNow);

        _tradeRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Trade> { ExistingTrade(tradeId) });
        _pnlRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PnLSnapshot> { older, latest });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradePnLQuery(tradeId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.PnLId.Should().Be(latest.PnLId);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectPnLFields()
    {
        var tradeId  = Guid.NewGuid();
        var snapshot = MakeSnapshot(DateTime.UtcNow);

        _tradeRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Trade> { ExistingTrade(tradeId) });
        _pnlRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PnLSnapshot, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PnLSnapshot> { snapshot });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTradePnLQuery(tradeId), CancellationToken.None);

        result!.RealizedPnL.Should().Be(snapshot.RealizedPnL);
        result.UnrealizedPnL.Should().Be(snapshot.UnrealizedPnL);
        result.TotalPnL.Should().Be(snapshot.TotalPnL);
        result.NetPnL.Should().Be(snapshot.NetPnL);
    }
}
