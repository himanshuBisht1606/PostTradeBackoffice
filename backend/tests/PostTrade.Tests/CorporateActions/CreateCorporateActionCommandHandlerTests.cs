using PostTrade.Application.Features.CorporateActions.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.CorporateActions;

public class CreateCorporateActionCommandHandlerTests
{
    private readonly Mock<IRepository<CorporateAction>> _repo          = new();
    private readonly Mock<IUnitOfWork>                  _unitOfWork    = new();
    private readonly Mock<ITenantContext>               _tenantContext = new();

    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private CreateCorporateActionCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<CorporateAction>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((CorporateAction a, CancellationToken _) => a);
        return new CreateCorporateActionCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateCorporateActionCommand ValidCommand() => new(
        InstrumentId:     InstrumentId,
        ActionType:       CorporateActionType.Dividend,
        AnnouncementDate: DateTime.Today,
        ExDate:           DateTime.Today.AddDays(5),
        RecordDate:       DateTime.Today.AddDays(7),
        PaymentDate:      DateTime.Today.AddDays(10),
        DividendAmount:   2.50m,
        BonusRatio:       null,
        SplitRatio:       null,
        RightsRatio:      null,
        RightsPrice:      null
    );

    [Fact]
    public async Task Handle_ShouldSetStatusToAnnounced()
    {
        CorporateAction? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<CorporateAction>(), It.IsAny<CancellationToken>()))
             .Callback<CorporateAction, CancellationToken>((a, _) => captured = a)
             .ReturnsAsync((CorporateAction a, CancellationToken _) => a);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.Status.Should().Be(CorporateActionStatus.Announced);
    }

    [Fact]
    public async Task Handle_ShouldSetIsProcessedToFalse()
    {
        CorporateAction? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<CorporateAction>(), It.IsAny<CancellationToken>()))
             .Callback<CorporateAction, CancellationToken>((a, _) => captured = a)
             .ReturnsAsync((CorporateAction a, CancellationToken _) => a);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        CorporateAction? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<CorporateAction>(), It.IsAny<CancellationToken>()))
             .Callback<CorporateAction, CancellationToken>((a, _) => captured = a)
             .ReturnsAsync((CorporateAction a, CancellationToken _) => a);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCorporateActionId()
    {
        CorporateAction? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<CorporateAction>(), It.IsAny<CancellationToken>()))
             .Callback<CorporateAction, CancellationToken>((a, _) => captured = a)
             .ReturnsAsync((CorporateAction a, CancellationToken _) => a);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.CorporateActionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<CorporateAction>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
