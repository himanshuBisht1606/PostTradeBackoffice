using PostTrade.Application.Features.MasterSetup.Brokers.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Brokers;

public class CreateBrokerCommandHandlerTests
{
    private readonly Mock<IRepository<Broker>> _repo          = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork    = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private CreateBrokerCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Broker b, CancellationToken _) => b);
        return new CreateBrokerCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateBrokerCommand ValidCommand() => new(
        BrokerCode:         "BRK001",
        BrokerName:         "Alpha Brokers",
        ContactEmail:       "contact@alpha.com",
        ContactPhone:       "9876543210",
        SEBIRegistrationNo: "SEBI123",
        Address:            null,
        PAN:                null,
        GST:                null
    );

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        Broker? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()))
             .Callback<Broker, CancellationToken>((b, _) => captured = b)
             .ReturnsAsync((Broker b, CancellationToken _) => b);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToActive()
    {
        Broker? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()))
             .Callback<Broker, CancellationToken>((b, _) => captured = b)
             .ReturnsAsync((Broker b, CancellationToken _) => b);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.Status.Should().Be(BrokerStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewBrokerId()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.BrokerId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.BrokerCode.Should().Be(command.BrokerCode);
        result.BrokerName.Should().Be(command.BrokerName);
        result.ContactEmail.Should().Be(command.ContactEmail);
        result.ContactPhone.Should().Be(command.ContactPhone);
        result.Status.Should().Be(BrokerStatus.Active);
    }
}
