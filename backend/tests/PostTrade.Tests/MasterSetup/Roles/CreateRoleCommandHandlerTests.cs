using PostTrade.Application.Features.MasterSetup.Roles.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Roles;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRepository<Role>> _repo          = new();
    private readonly Mock<IUnitOfWork>       _unitOfWork    = new();
    private readonly Mock<ITenantContext>    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private CreateRoleCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Role r, CancellationToken _) => r);
        return new CreateRoleCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldSetIsSystemRoleToFalse()
    {
        Role? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
             .Callback<Role, CancellationToken>((r, _) => captured = r)
             .ReturnsAsync((Role r, CancellationToken _) => r);

        await handler.Handle(new CreateRoleCommand("Analyst", "Read-only role"), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.IsSystemRole.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        Role? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
             .Callback<Role, CancellationToken>((r, _) => captured = r)
             .ReturnsAsync((Role r, CancellationToken _) => r);

        await handler.Handle(new CreateRoleCommand("Analyst", null), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewRoleId()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(new CreateRoleCommand("Analyst", null), CancellationToken.None);

        result.RoleId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(new CreateRoleCommand("Analyst", null), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = new CreateRoleCommand("Analyst", "Read-only access");
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.RoleName.Should().Be(command.RoleName);
        result.Description.Should().Be(command.Description);
        result.IsSystemRole.Should().BeFalse();
    }
}
