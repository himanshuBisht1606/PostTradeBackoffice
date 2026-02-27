using PostTrade.Application.Features.MasterSetup.Exchanges.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Exchanges;

public class CreateExchangeCommandHandlerTests
{
    private readonly Mock<IRepository<Exchange>> _repo          = new();
    private readonly Mock<IUnitOfWork>           _unitOfWork    = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private CreateExchangeCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Exchange e, CancellationToken _) => e);
        return new CreateExchangeCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateExchangeCommand ValidCommand() => new(
        ExchangeCode:     "NSE",
        ExchangeName:     "National Stock Exchange",
        Country:          "India",
        TimeZone:         "Asia/Kolkata",
        TradingStartTime: null,
        TradingEndTime:   null
    );

    [Fact]
    public async Task Handle_ShouldSetIsActiveToTrue()
    {
        Exchange? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()))
             .Callback<Exchange, CancellationToken>((e, _) => captured = e)
             .ReturnsAsync((Exchange e, CancellationToken _) => e);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        Exchange? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()))
             .Callback<Exchange, CancellationToken>((e, _) => captured = e)
             .ReturnsAsync((Exchange e, CancellationToken _) => e);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewExchangeId()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.ExchangeId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Exchange>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.ExchangeCode.Should().Be(command.ExchangeCode);
        result.ExchangeName.Should().Be(command.ExchangeName);
        result.Country.Should().Be(command.Country);
        result.IsActive.Should().BeTrue();
    }
}
