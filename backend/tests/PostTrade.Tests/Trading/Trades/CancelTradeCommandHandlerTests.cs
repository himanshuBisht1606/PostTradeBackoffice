using PostTrade.Application.Features.Trading.Trades.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Trading.Trades;

public class CancelTradeCommandHandlerTests
{
    private readonly Mock<IRepository<Trade>> _repo          = new();
    private readonly Mock<IUnitOfWork>        _unitOfWork    = new();
    private readonly Mock<ITenantContext>     _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private CancelTradeCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new CancelTradeCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private Trade ExistingTrade(Guid tradeId, TradeStatus status = TradeStatus.Pending) => new()
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
        Status       = status,
        Source       = TradeSource.Manual,
        NetAmount    = 25000m
    };

    [Fact]
    public async Task Handle_WhenTradeNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new CancelTradeCommand(Guid.NewGuid(), "Test reason"), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTradeIsPending_ShouldCancelSuccessfully()
    {
        var tradeId = Guid.NewGuid();
        var trade   = ExistingTrade(tradeId, TradeStatus.Pending);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        var result  = await handler.Handle(new CancelTradeCommand(tradeId, "Client request"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(TradeStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WhenTradeIsValidated_ShouldCancelSuccessfully()
    {
        var tradeId = Guid.NewGuid();
        var trade   = ExistingTrade(tradeId, TradeStatus.Validated);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        var result  = await handler.Handle(new CancelTradeCommand(tradeId, "Client request"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(TradeStatus.Cancelled);
    }

    [Theory]
    [InlineData(TradeStatus.Settled)]
    [InlineData(TradeStatus.Cancelled)]
    [InlineData(TradeStatus.Rejected)]
    [InlineData(TradeStatus.Amended)]
    public async Task Handle_WhenTradeIsNotCancellable_ShouldThrowInvalidOperationException(TradeStatus status)
    {
        var tradeId = Guid.NewGuid();
        var trade   = ExistingTrade(tradeId, status);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        var act     = () => handler.Handle(new CancelTradeCommand(tradeId, "reason"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldSetRejectionReason()
    {
        var tradeId = Guid.NewGuid();
        var trade   = ExistingTrade(tradeId, TradeStatus.Pending);
        const string reason = "Client cancelled order";

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        await handler.Handle(new CancelTradeCommand(tradeId, reason), CancellationToken.None);

        trade.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSave()
    {
        var tradeId = Guid.NewGuid();
        var trade   = ExistingTrade(tradeId, TradeStatus.Pending);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Trade> { trade });

        var handler = CreateHandler();
        await handler.Handle(new CancelTradeCommand(tradeId, "reason"), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(trade, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
