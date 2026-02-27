using PostTrade.Application.Features.Trading.Trades.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Trading.Trades;

public class BookTradeCommandHandlerTests
{
    private readonly Mock<IRepository<Trade>> _repo          = new();
    private readonly Mock<IUnitOfWork>        _unitOfWork    = new();
    private readonly Mock<ITenantContext>     _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid BrokerId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private BookTradeCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Trade t, CancellationToken _) => t);
        return new BookTradeCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static BookTradeCommand ValidCommand() => new(
        BrokerId:        BrokerId,
        ClientId:        ClientId,
        InstrumentId:    InstrumentId,
        Side:            TradeSide.Buy,
        Quantity:        100,
        Price:           250.50m,
        TradeDate:       DateTime.Today,
        SettlementNo:    "SN001",
        Source:          TradeSource.Manual,
        ExchangeTradeNo: null,
        SourceReference: null
    );

    [Fact]
    public async Task Handle_ShouldCalculateTradeValueAsQuantityTimesPrice()
    {
        Trade? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
             .Callback<Trade, CancellationToken>((t, _) => captured = t)
             .ReturnsAsync((Trade t, CancellationToken _) => t);

        var command = ValidCommand();
        await handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TradeValue.Should().Be(command.Quantity * command.Price);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToPending()
    {
        Trade? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
             .Callback<Trade, CancellationToken>((t, _) => captured = t)
             .ReturnsAsync((Trade t, CancellationToken _) => t);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.Status.Should().Be(TradeStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldGenerateTradeNo()
    {
        Trade? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
             .Callback<Trade, CancellationToken>((t, _) => captured = t)
             .ReturnsAsync((Trade t, CancellationToken _) => t);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TradeNo.Should().StartWith("TRD");
        captured.TradeNo.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        Trade? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
             .Callback<Trade, CancellationToken>((t, _) => captured = t)
             .ReturnsAsync((Trade t, CancellationToken _) => t);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldSetNetAmountToTradeValue()
    {
        Trade? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
             .Callback<Trade, CancellationToken>((t, _) => captured = t)
             .ReturnsAsync((Trade t, CancellationToken _) => t);

        var command = ValidCommand();
        await handler.Handle(command, CancellationToken.None);

        captured!.NetAmount.Should().Be(command.Quantity * command.Price);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.BrokerId.Should().Be(command.BrokerId);
        result.ClientId.Should().Be(command.ClientId);
        result.InstrumentId.Should().Be(command.InstrumentId);
        result.Side.Should().Be(command.Side);
        result.Quantity.Should().Be(command.Quantity);
        result.Price.Should().Be(command.Price);
        result.TradeValue.Should().Be(command.Quantity * command.Price);
        result.Status.Should().Be(TradeStatus.Pending);
    }
}
