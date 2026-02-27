using PostTrade.Application.Features.MasterSetup.Clients.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Clients;

public class CreateClientCommandHandlerTests
{
    private readonly Mock<IRepository<Client>> _repo         = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork   = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private CreateClientCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client c, CancellationToken _) => c);
        return new CreateClientCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateClientCommand ValidCommand() => new(
        BrokerId:       Guid.NewGuid(),
        BranchId:       null,
        ClientCode:     "CLT001",
        ClientName:     "John Doe",
        Email:          "john@example.com",
        Phone:          "9876543210",
        ClientType:     ClientType.Individual,
        PAN:            null,
        Aadhaar:        null,
        DPId:           null,
        DematAccountNo: null,
        Depository:     null,
        Address:        null,
        StateCode:      null,
        StateName:      null,
        BankAccountNo:  null,
        BankName:       null,
        BankIFSC:       null
    );

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        Client? captured = null;
        var handler = CreateHandler(); // sets up default AddAsync

        // Override AFTER CreateHandler so this callback setup wins
        _repo.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Callback<Client, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Client c, CancellationToken _) => c);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.TenantId.Should().Be(TenantId);
        result.BrokerId.Should().Be(command.BrokerId);
        result.ClientCode.Should().Be(command.ClientCode);
        result.ClientName.Should().Be(command.ClientName);
        result.Email.Should().Be(command.Email);
        result.Phone.Should().Be(command.Phone);
        result.ClientType.Should().Be(command.ClientType);
        result.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewClientId()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.ClientId.Should().NotBe(Guid.Empty);
    }
}
