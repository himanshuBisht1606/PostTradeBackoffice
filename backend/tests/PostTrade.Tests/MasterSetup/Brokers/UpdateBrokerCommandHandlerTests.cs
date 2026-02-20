using PostTrade.Application.Features.MasterSetup.Brokers.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Brokers;

public class UpdateBrokerCommandHandlerTests
{
    private readonly Mock<IRepository<Broker>> _repo          = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork    = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private UpdateBrokerCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new UpdateBrokerCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private Broker ExistingBroker(Guid brokerId) => new()
    {
        BrokerId     = brokerId,
        TenantId     = TenantId,
        BrokerCode   = "BRK001",
        BrokerName   = "Original Name",
        ContactEmail = "old@broker.com",
        ContactPhone = "1111111111",
        Status       = BrokerStatus.Active
    };

    private static UpdateBrokerCommand ValidCommand(Guid brokerId) => new(
        BrokerId:           brokerId,
        BrokerName:         "Updated Name",
        ContactEmail:       "new@broker.com",
        ContactPhone:       "9999999999",
        Status:             BrokerStatus.Active,
        SEBIRegistrationNo: "SEBI999",
        Address:            "New Address",
        PAN:                null,
        GST:                null
    );

    [Fact]
    public async Task Handle_WhenBrokerNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker>());

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBrokerExists_ShouldUpdateFields()
    {
        var brokerId = Guid.NewGuid();
        var broker   = ExistingBroker(brokerId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker> { broker });

        var command = ValidCommand(brokerId);
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.BrokerName.Should().Be(command.BrokerName);
        result.ContactEmail.Should().Be(command.ContactEmail);
        result.ContactPhone.Should().Be(command.ContactPhone);
    }

    [Fact]
    public async Task Handle_WhenBrokerExists_ShouldCallUpdateAndSave()
    {
        var brokerId = Guid.NewGuid();
        var broker   = ExistingBroker(brokerId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker> { broker });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(brokerId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(broker, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeBrokerCode()
    {
        var brokerId     = Guid.NewGuid();
        var broker       = ExistingBroker(brokerId);
        var originalCode = broker.BrokerCode;

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker> { broker });

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(brokerId), CancellationToken.None);

        result!.BrokerCode.Should().Be(originalCode);
    }
}
