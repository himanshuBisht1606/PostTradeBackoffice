using PostTrade.Application.Features.Reconciliation.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;
using ReconEntity = PostTrade.Domain.Entities.Reconciliation.Reconciliation;

namespace PostTrade.Tests.Reconciliation;

public class RunReconciliationCommandHandlerTests
{
    private readonly Mock<IRepository<ReconEntity>>   _reconRepo     = new();
    private readonly Mock<IRepository<ReconException>> _exceptionRepo = new();
    private readonly Mock<IUnitOfWork>                _unitOfWork    = new();
    private readonly Mock<ITenantContext>             _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private RunReconciliationCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _reconRepo.Setup(r => r.AddAsync(It.IsAny<ReconEntity>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((ReconEntity r, CancellationToken _) => r);
        _exceptionRepo.Setup(r => r.AddAsync(It.IsAny<ReconException>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((ReconException e, CancellationToken _) => e);
        return new RunReconciliationCommandHandler(
            _reconRepo.Object, _exceptionRepo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static RunReconciliationCommand MatchedCommand() => new(
        ReconDate:      DateTime.Today,
        SettlementNo:   "SN001",
        ReconType:      ReconType.Trade,
        SystemValue:    1000m,
        ExchangeValue:  1001m, // diff = 1, tolerance = 2 → Matched
        ToleranceLimit: 2m,
        Comments:       null
    );

    private static RunReconciliationCommand MismatchedCommand() => new(
        ReconDate:      DateTime.Today,
        SettlementNo:   "SN001",
        ReconType:      ReconType.Trade,
        SystemValue:    1000m,
        ExchangeValue:  1010m, // diff = 10, tolerance = 2 → Mismatched
        ToleranceLimit: 2m,
        Comments:       null
    );

    [Fact]
    public async Task Handle_WhenDifferenceWithinTolerance_ShouldSetStatusMatched()
    {
        ReconEntity? captured = null;
        var handler = CreateHandler();
        _reconRepo.Setup(r => r.AddAsync(It.IsAny<ReconEntity>(), It.IsAny<CancellationToken>()))
                  .Callback<ReconEntity, CancellationToken>((r, _) => captured = r)
                  .ReturnsAsync((ReconEntity r, CancellationToken _) => r);

        await handler.Handle(MatchedCommand(), CancellationToken.None);

        captured!.Status.Should().Be(ReconStatus.Matched);
    }

    [Fact]
    public async Task Handle_WhenDifferenceExceedsTolerance_ShouldSetStatusMismatched()
    {
        ReconEntity? captured = null;
        var handler = CreateHandler();
        _reconRepo.Setup(r => r.AddAsync(It.IsAny<ReconEntity>(), It.IsAny<CancellationToken>()))
                  .Callback<ReconEntity, CancellationToken>((r, _) => captured = r)
                  .ReturnsAsync((ReconEntity r, CancellationToken _) => r);

        await handler.Handle(MismatchedCommand(), CancellationToken.None);

        captured!.Status.Should().Be(ReconStatus.Mismatched);
    }

    [Fact]
    public async Task Handle_WhenMismatched_ShouldCreateReconException()
    {
        var handler = CreateHandler();
        await handler.Handle(MismatchedCommand(), CancellationToken.None);

        _exceptionRepo.Verify(r => r.AddAsync(It.IsAny<ReconException>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMatched_ShouldNotCreateReconException()
    {
        var handler = CreateHandler();
        await handler.Handle(MatchedCommand(), CancellationToken.None);

        _exceptionRepo.Verify(r => r.AddAsync(It.IsAny<ReconException>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        ReconEntity? captured = null;
        var handler = CreateHandler();
        _reconRepo.Setup(r => r.AddAsync(It.IsAny<ReconEntity>(), It.IsAny<CancellationToken>()))
                  .Callback<ReconEntity, CancellationToken>((r, _) => captured = r)
                  .ReturnsAsync((ReconEntity r, CancellationToken _) => r);

        await handler.Handle(MatchedCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }
}
