using PostTrade.Application.Features.MasterSetup.Users.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _repo            = new();
    private readonly Mock<IUnitOfWork>       _unitOfWork      = new();
    private readonly Mock<ITenantContext>    _tenantContext   = new();
    private readonly Mock<IPasswordService>  _passwordService = new();

    private static readonly Guid   TenantId    = Guid.NewGuid();
    private const           string HashedValue = "hashed-password-value";

    private CreateUserCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _passwordService.Setup(p => p.HashPassword(It.IsAny<string>())).Returns(HashedValue);
        _repo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((User u, CancellationToken _) => u);
        return new CreateUserCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object, _passwordService.Object);
    }

    private static CreateUserCommand ValidCommand() => new(
        Username:  "jdoe",
        Email:     "jdoe@example.com",
        Password:  "secret123",
        FirstName: "John",
        LastName:  "Doe",
        Phone:     "9876543210"
    );

    [Fact]
    public async Task Handle_ShouldHashPasswordWithPasswordService()
    {
        User? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
             .Callback<User, CancellationToken>((u, _) => captured = u)
             .ReturnsAsync((User u, CancellationToken _) => u);

        var command = ValidCommand();
        await handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.PasswordHash.Should().Be(HashedValue);
        captured.PasswordHash.Should().NotBe(command.Password);
    }

    [Fact]
    public async Task Handle_ShouldCallPasswordServiceWithPlainPassword()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _passwordService.Verify(p => p.HashPassword(command.Password), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToActive()
    {
        User? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
             .Callback<User, CancellationToken>((u, _) => captured = u)
             .ReturnsAsync((User u, CancellationToken _) => u);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        User? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
             .Callback<User, CancellationToken>((u, _) => captured = u)
             .ReturnsAsync((User u, CancellationToken _) => u);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewUserId()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.Username.Should().Be(command.Username);
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Status.Should().Be(UserStatus.Active);
    }
}
