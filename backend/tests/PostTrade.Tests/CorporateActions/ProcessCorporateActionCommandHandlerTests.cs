using PostTrade.Application.Features.CorporateActions.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.CorporateActions;

public class ProcessCorporateActionCommandHandlerTests
{
    private readonly Mock<IRepository<CorporateAction>> _repo          = new();
    private readonly Mock<IUnitOfWork>                  _unitOfWork    = new();
    private readonly Mock<ITenantContext>               _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private ProcessCorporateActionCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new ProcessCorporateActionCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private CorporateAction ExistingAction(Guid id, CorporateActionStatus status = CorporateActionStatus.Announced) => new()
    {
        CorporateActionId = id,
        TenantId          = TenantId,
        InstrumentId      = Guid.NewGuid(),
        ActionType        = CorporateActionType.Dividend,
        AnnouncementDate  = DateTime.Today,
        ExDate            = DateTime.Today.AddDays(5),
        RecordDate        = DateTime.Today.AddDays(7),
        DividendAmount    = 2.50m,
        Status            = status,
        IsProcessed       = false
    };

    [Fact]
    public async Task Handle_WhenNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new ProcessCorporateActionCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(CorporateActionStatus.Processing)]
    [InlineData(CorporateActionStatus.Completed)]
    [InlineData(CorporateActionStatus.Cancelled)]
    public async Task Handle_WhenNotInAnnouncedStatus_ShouldThrowInvalidOperationException(CorporateActionStatus status)
    {
        var id     = Guid.NewGuid();
        var action = ExistingAction(id, status);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction> { action });

        var handler = CreateHandler();
        var act     = () => handler.Handle(new ProcessCorporateActionCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldTransitionToCompleted()
    {
        var id     = Guid.NewGuid();
        var action = ExistingAction(id);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction> { action });

        var handler = CreateHandler();
        var result  = await handler.Handle(new ProcessCorporateActionCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(CorporateActionStatus.Completed);
    }

    [Fact]
    public async Task Handle_ShouldSetIsProcessedTrue()
    {
        var id     = Guid.NewGuid();
        var action = ExistingAction(id);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction> { action });

        var handler = CreateHandler();
        await handler.Handle(new ProcessCorporateActionCommand(id), CancellationToken.None);

        action.IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetProcessedAt()
    {
        var id     = Guid.NewGuid();
        var action = ExistingAction(id);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction> { action });

        var handler = CreateHandler();
        await handler.Handle(new ProcessCorporateActionCommand(id), CancellationToken.None);

        action.ProcessedAt.Should().NotBeNull();
        action.ProcessedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
