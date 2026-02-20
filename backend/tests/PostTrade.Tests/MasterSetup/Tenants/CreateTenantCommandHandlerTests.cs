using PostTrade.Application.Features.MasterSetup.Tenants.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Tenants;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant>> _repo       = new();
    private readonly Mock<IUnitOfWork>         _unitOfWork = new();

    private CreateTenantCommandHandler CreateHandler()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant t, CancellationToken _) => t);
        return new CreateTenantCommandHandler(_repo.Object, _unitOfWork.Object);
    }

    private static CreateTenantCommand ValidCommand() => new(
        TenantCode:    "TEN001",
        TenantName:    "Demo Corp",
        ContactEmail:  "admin@democorp.com",
        ContactPhone:  "9876543210",
        Address:       null,
        City:          null,
        Country:       null,
        TaxId:         null
    );

    [Fact]
    public async Task Handle_ShouldSetStatusToActive()
    {
        Tenant? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
             .Callback<Tenant, CancellationToken>((t, _) => captured = t)
             .ReturnsAsync((Tenant t, CancellationToken _) => t);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewTenantId()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.TenantId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.TenantCode.Should().Be(command.TenantCode);
        result.TenantName.Should().Be(command.TenantName);
        result.ContactEmail.Should().Be(command.ContactEmail);
        result.ContactPhone.Should().Be(command.ContactPhone);
        result.Status.Should().Be(TenantStatus.Active);
    }
}
