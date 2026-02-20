using PostTrade.Application.Features.MasterSetup.Tenants.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Tenants;

public class UpdateTenantCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant>> _repo       = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork = new();

    private UpdateTenantCommandHandler CreateHandler()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new UpdateTenantCommandHandler(_repo.Object, _unitOfWork.Object);
    }

    private static Tenant ExistingTenant(Guid tenantId) => new()
    {
        TenantId     = tenantId,
        TenantCode   = "TEN001",
        TenantName   = "Original Name",
        ContactEmail = "old@corp.com",
        ContactPhone = "1111111111",
        Status       = TenantStatus.Active
    };

    private static UpdateTenantCommand ValidCommand(Guid tenantId) => new(
        TenantId:     tenantId,
        TenantName:   "Updated Name",
        ContactEmail: "new@corp.com",
        ContactPhone: "9999999999",
        Status:       TenantStatus.Active,
        Address:      "123 Main St",
        City:         "Mumbai",
        Country:      "India",
        TaxId:        null
    );

    [Fact]
    public async Task Handle_WhenTenantNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant>());

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantExists_ShouldUpdateFields()
    {
        var tenantId = Guid.NewGuid();
        var tenant   = ExistingTenant(tenantId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var command = ValidCommand(tenantId);
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TenantName.Should().Be(command.TenantName);
        result.ContactEmail.Should().Be(command.ContactEmail);
        result.ContactPhone.Should().Be(command.ContactPhone);
        result.City.Should().Be(command.City);
    }

    [Fact]
    public async Task Handle_WhenTenantExists_ShouldCallUpdateAndSave()
    {
        var tenantId = Guid.NewGuid();
        var tenant   = ExistingTenant(tenantId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(tenantId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeTenantCode()
    {
        var tenantId     = Guid.NewGuid();
        var tenant       = ExistingTenant(tenantId);
        var originalCode = tenant.TenantCode;

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(tenantId), CancellationToken.None);

        result!.TenantCode.Should().Be(originalCode);
    }
}
