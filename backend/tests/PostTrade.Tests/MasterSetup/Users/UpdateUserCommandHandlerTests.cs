using PostTrade.Application.Features.MasterSetup.Users.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _repo          = new();
    private readonly Mock<IUnitOfWork>       _unitOfWork    = new();
    private readonly Mock<ITenantContext>    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private UpdateUserCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new UpdateUserCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private User ExistingUser(Guid userId) => new()
    {
        UserId       = userId,
        TenantId     = TenantId,
        Username     = "jdoe",
        Email        = "old@example.com",
        PasswordHash = "hashed",
        FirstName    = "John",
        LastName     = "Doe",
        Phone        = "1111111111",
        Status       = UserStatus.Active
    };

    private static UpdateUserCommand ValidCommand(Guid userId) => new(
        UserId:    userId,
        Email:     "new@example.com",
        FirstName: "Jane",
        LastName:  "Smith",
        Phone:     "9999999999",
        Status:    UserStatus.Active
    );

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<User>());

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldUpdateFields()
    {
        var userId = Guid.NewGuid();
        var user   = ExistingUser(userId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<User> { user });

        var command = ValidCommand(userId);
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCallUpdateAndSave()
    {
        var userId = Guid.NewGuid();
        var user   = ExistingUser(userId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<User> { user });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(userId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeUsernameOrPasswordHash()
    {
        var userId          = Guid.NewGuid();
        var user            = ExistingUser(userId);
        var originalUsername = user.Username;
        var originalHash    = user.PasswordHash;

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<User> { user });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(userId), CancellationToken.None);

        user.Username.Should().Be(originalUsername);
        user.PasswordHash.Should().Be(originalHash);
    }
}
