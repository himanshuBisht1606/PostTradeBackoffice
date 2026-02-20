using PostTrade.Application.Features.MasterSetup.Tenants.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Tenants;

public class GetTenantByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Tenant>> _repo = new();

    private GetTenantByIdQueryHandler CreateHandler() =>
        new GetTenantByIdQueryHandler(_repo.Object);

    private static Tenant ExistingTenant(Guid tenantId) => new()
    {
        TenantId     = tenantId,
        TenantCode   = "TEN001",
        TenantName   = "Demo Corp",
        ContactEmail = "admin@corp.com",
        ContactPhone = "9876543210",
        Status       = TenantStatus.Active
    };

    [Fact]
    public async Task Handle_WhenTenantExists_ShouldReturnDto()
    {
        var tenantId = Guid.NewGuid();
        var tenant   = ExistingTenant(tenantId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTenantByIdQuery(tenantId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be(tenantId);
        result.TenantName.Should().Be(tenant.TenantName);
        result.ContactEmail.Should().Be(tenant.ContactEmail);
    }

    [Fact]
    public async Task Handle_WhenTenantNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTenantByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
