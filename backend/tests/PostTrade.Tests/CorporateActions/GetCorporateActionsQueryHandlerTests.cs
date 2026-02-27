using PostTrade.Application.Features.CorporateActions.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.CorporateActions;

public class GetCorporateActionsQueryHandlerTests
{
    private readonly Mock<IRepository<CorporateAction>> _repo          = new();
    private readonly Mock<ITenantContext>               _tenantContext = new();

    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetCorporateActionsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetCorporateActionsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static CorporateAction MakeAction(CorporateActionStatus status = CorporateActionStatus.Announced) => new()
    {
        CorporateActionId = Guid.NewGuid(),
        TenantId          = TenantId,
        InstrumentId      = InstrumentId,
        ActionType        = CorporateActionType.Dividend,
        AnnouncementDate  = DateTime.Today,
        ExDate            = DateTime.Today.AddDays(5),
        RecordDate        = DateTime.Today.AddDays(7),
        DividendAmount    = 2.50m,
        Status            = status,
        IsProcessed       = false
    };

    [Fact]
    public async Task Handle_ShouldReturnAllActionsForTenant()
    {
        var actions = new List<CorporateAction> { MakeAction(), MakeAction() };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(actions);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetCorporateActionsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoActions_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetCorporateActionsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByInstrumentId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction>());

        var handler = CreateHandler();
        await handler.Handle(new GetCorporateActionsQuery(InstrumentId: InstrumentId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CorporateAction, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<CorporateAction>());

        var handler = CreateHandler();
        await handler.Handle(new GetCorporateActionsQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
