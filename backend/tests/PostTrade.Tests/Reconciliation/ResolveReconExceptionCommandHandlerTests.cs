using PostTrade.Application.Features.Reconciliation.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Reconciliation;

public class ResolveReconExceptionCommandHandlerTests
{
    private readonly Mock<IRepository<ReconException>> _repo          = new();
    private readonly Mock<IUnitOfWork>                 _unitOfWork    = new();
    private readonly Mock<ITenantContext>              _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private ResolveReconExceptionCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new ResolveReconExceptionCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private ReconException ExistingException(Guid id, ExceptionStatus status = ExceptionStatus.Open) => new()
    {
        ExceptionId          = id,
        ReconId              = Guid.NewGuid(),
        TenantId             = TenantId,
        ExceptionType        = ExceptionType.QuantityMismatch,
        ExceptionDescription = "Test exception",
        ReferenceNo          = "SN001",
        Amount               = 500m,
        Status               = status
    };

    [Fact]
    public async Task Handle_WhenExceptionNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new ResolveReconExceptionCommand(Guid.NewGuid(), "resolution"), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(ExceptionStatus.Resolved)]
    [InlineData(ExceptionStatus.Closed)]
    public async Task Handle_WhenAlreadyResolvedOrClosed_ShouldThrowInvalidOperationException(ExceptionStatus status)
    {
        var id        = Guid.NewGuid();
        var exception = ExistingException(id, status);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException> { exception });

        var handler = CreateHandler();
        var act     = () => handler.Handle(new ResolveReconExceptionCommand(id, "resolution"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldSetResolutionText()
    {
        var id        = Guid.NewGuid();
        var exception = ExistingException(id);
        const string resolution = "Corrected trade entry";

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException> { exception });

        var handler = CreateHandler();
        await handler.Handle(new ResolveReconExceptionCommand(id, resolution), CancellationToken.None);

        exception.Resolution.Should().Be(resolution);
    }

    [Fact]
    public async Task Handle_ShouldSetResolvedAt()
    {
        var id        = Guid.NewGuid();
        var exception = ExistingException(id);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException> { exception });

        var handler = CreateHandler();
        await handler.Handle(new ResolveReconExceptionCommand(id, "resolution"), CancellationToken.None);

        exception.ResolvedAt.Should().NotBeNull();
        exception.ResolvedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToResolved()
    {
        var id        = Guid.NewGuid();
        var exception = ExistingException(id);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException> { exception });

        var handler = CreateHandler();
        var result  = await handler.Handle(new ResolveReconExceptionCommand(id, "resolution"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(ExceptionStatus.Resolved);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSave()
    {
        var id        = Guid.NewGuid();
        var exception = ExistingException(id);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException> { exception });

        var handler = CreateHandler();
        await handler.Handle(new ResolveReconExceptionCommand(id, "resolution"), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(exception, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
