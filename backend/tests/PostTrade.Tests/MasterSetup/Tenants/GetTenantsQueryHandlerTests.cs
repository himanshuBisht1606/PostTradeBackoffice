using PostTrade.Application.Features.MasterSetup.Tenants.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Tenants;

public class GetTenantsQueryHandlerTests
{
    private readonly Mock<IRepository<Tenant>> _repo = new();

    private GetTenantsQueryHandler CreateHandler() =>
        new GetTenantsQueryHandler(_repo.Object);

    private static Tenant MakeTenant(int index = 1) => new()
    {
        TenantId     = Guid.NewGuid(),
        TenantCode   = $"TEN00{index}",
        TenantName   = $"Tenant {index}",
        ContactEmail = $"tenant{index}@corp.com",
        ContactPhone = "9876543210",
        Status       = TenantStatus.Active
    };

    [Fact]
    public async Task Handle_ShouldReturnAllTenants()
    {
        var tenants = new List<Tenant> { MakeTenant(1), MakeTenant(2), MakeTenant(3) };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenants);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTenantsQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenNoTenants_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetTenantsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var tenant = MakeTenant(1);

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Tenant> { tenant });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetTenantsQuery(), CancellationToken.None)).First();

        result.TenantId.Should().Be(tenant.TenantId);
        result.TenantCode.Should().Be(tenant.TenantCode);
        result.TenantName.Should().Be(tenant.TenantName);
        result.ContactEmail.Should().Be(tenant.ContactEmail);
        result.Status.Should().Be(tenant.Status);
    }
}
