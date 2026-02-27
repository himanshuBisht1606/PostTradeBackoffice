using PostTrade.Application.Features.MasterSetup.Tenants.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Tenants;

public class DeleteTenantCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant>> _repo       = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork = new();

    private DeleteTenantCommandHandler CreateHandler()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new DeleteTenantCommandHandler(_repo.Object, _unitOfWork.Object);
    }

    private static Tenant ExistingTenant(Guid tenantId) => new()
    {
        TenantId     = tenantId,
        TenantCode   = "TEN001",
        TenantName   = "Demo Corp",
        ContactEmail = "admin@corp.com",
        ContactPhone = "9876543210",
        Status       = TenantStatus.Active,
        IsDeleted    = false
    };

    [Fact]
    public async Task Handle_WhenTenantNotFound_ShouldReturnFalse()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new DeleteTenantCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantExists_ShouldReturnTrue()
    {
        var tenantId = Guid.NewGuid();
        var tenant   = ExistingTenant(tenantId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        var result  = await handler.Handle(new DeleteTenantCommand(tenantId), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToInactiveAndIsDeletedTrue()
    {
        var tenantId = Guid.NewGuid();
        var tenant   = ExistingTenant(tenantId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        await handler.Handle(new DeleteTenantCommand(tenantId), CancellationToken.None);

        tenant.Status.Should().Be(TenantStatus.Inactive);
        tenant.IsDeleted.Should().BeTrue();
        tenant.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSave()
    {
        var tenantId = Guid.NewGuid();
        var tenant   = ExistingTenant(tenantId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        await handler.Handle(new DeleteTenantCommand(tenantId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
