using PostTrade.Application.Features.MasterSetup.Roles.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Roles;

public class GetRolesQueryHandlerTests
{
    private readonly Mock<IRepository<Role>> _repo          = new();
    private readonly Mock<ITenantContext>    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetRolesQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetRolesQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Role MakeRole(int index = 1) => new()
    {
        RoleId       = Guid.NewGuid(),
        TenantId     = TenantId,
        RoleName     = $"Role{index}",
        Description  = $"Description {index}",
        IsSystemRole = false
    };

    [Fact]
    public async Task Handle_ShouldReturnAllRolesForTenant()
    {
        var roles = new List<Role> { MakeRole(1), MakeRole(2) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(roles);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoRoles_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Role>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var role = MakeRole(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Role> { role });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetRolesQuery(), CancellationToken.None)).First();

        result.RoleId.Should().Be(role.RoleId);
        result.TenantId.Should().Be(role.TenantId);
        result.RoleName.Should().Be(role.RoleName);
        result.Description.Should().Be(role.Description);
        result.IsSystemRole.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Role>());

        var handler = CreateHandler();
        await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
