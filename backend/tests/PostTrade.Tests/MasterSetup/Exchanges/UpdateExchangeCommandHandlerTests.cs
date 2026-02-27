using PostTrade.Application.Features.MasterSetup.Exchanges.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Exchanges;

public class UpdateExchangeCommandHandlerTests
{
    private readonly Mock<IRepository<Exchange>> _repo          = new();
    private readonly Mock<IUnitOfWork>           _unitOfWork    = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private UpdateExchangeCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new UpdateExchangeCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private Exchange ExistingExchange(Guid exchangeId) => new()
    {
        ExchangeId   = exchangeId,
        TenantId     = TenantId,
        ExchangeCode = "NSE",
        ExchangeName = "National Stock Exchange",
        Country      = "India",
        IsActive     = true
    };

    private static UpdateExchangeCommand ValidCommand(Guid exchangeId) => new(
        ExchangeId:       exchangeId,
        ExchangeName:     "NSE Updated",
        Country:          "India",
        TimeZone:         "Asia/Kolkata",
        TradingStartTime: null,
        TradingEndTime:   null,
        IsActive:         true
    );

    [Fact]
    public async Task Handle_WhenExchangeNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Exchange>());

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExchangeExists_ShouldUpdateFields()
    {
        var exchangeId = Guid.NewGuid();
        var exchange   = ExistingExchange(exchangeId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Exchange> { exchange });

        var command = ValidCommand(exchangeId);
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ExchangeName.Should().Be(command.ExchangeName);
        result.Country.Should().Be(command.Country);
        result.TimeZone.Should().Be(command.TimeZone);
    }

    [Fact]
    public async Task Handle_WhenExchangeExists_ShouldCallUpdateAndSave()
    {
        var exchangeId = Guid.NewGuid();
        var exchange   = ExistingExchange(exchangeId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Exchange> { exchange });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(exchangeId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(exchange, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeExchangeCode()
    {
        var exchangeId   = Guid.NewGuid();
        var exchange     = ExistingExchange(exchangeId);
        var originalCode = exchange.ExchangeCode;

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Exchange> { exchange });

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(exchangeId), CancellationToken.None);

        result!.ExchangeCode.Should().Be(originalCode);
    }
}
