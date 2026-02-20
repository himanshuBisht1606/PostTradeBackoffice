using PostTrade.Application.Features.MasterSetup.Clients.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Clients;

public class UpdateClientCommandHandlerTests
{
    private readonly Mock<IRepository<Client>> _repo         = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork   = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private UpdateClientCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new UpdateClientCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private Client ExistingClient(Guid clientId) => new()
    {
        ClientId   = clientId,
        TenantId   = TenantId,
        BrokerId   = Guid.NewGuid(),
        ClientCode = "CLT001",
        ClientName = "Original Name",
        Email      = "old@example.com",
        Phone      = "1111111111",
        ClientType = ClientType.Individual,
        Status     = ClientStatus.Active
    };

    private static UpdateClientCommand ValidCommand(Guid clientId) => new(
        ClientId:      clientId,
        ClientName:    "Updated Name",
        Email:         "new@example.com",
        Phone:         "9999999999",
        Status:        ClientStatus.Active,
        PAN:           null,
        Address:       "New Address",
        BankAccountNo: null,
        BankName:      null,
        BankIFSC:      null
    );

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldReturnNull()
    {
        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>());

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldUpdateFields()
    {
        var clientId = Guid.NewGuid();
        var client   = ExistingClient(clientId);

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var command = ValidCommand(clientId);
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ClientName.Should().Be(command.ClientName);
        result.Email.Should().Be(command.Email);
        result.Phone.Should().Be(command.Phone);
        result.Address.Should().Be(command.Address);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldCallUpdateAndSave()
    {
        var clientId = Guid.NewGuid();
        var client   = ExistingClient(clientId);

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(clientId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(client, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeClientCodeOrType()
    {
        var clientId = Guid.NewGuid();
        var client   = ExistingClient(clientId);
        var originalCode = client.ClientCode;
        var originalType = client.ClientType;

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(clientId), CancellationToken.None);

        result!.ClientCode.Should().Be(originalCode);
        result.ClientType.Should().Be(originalType);
    }
}
