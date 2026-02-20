using PostTrade.Application.Features.Settlement.Obligations.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Settlement;

public class SettleObligationCommandHandlerTests
{
    private readonly Mock<IRepository<SettlementObligation>> _repo          = new();
    private readonly Mock<IUnitOfWork>                       _unitOfWork    = new();
    private readonly Mock<ITenantContext>                    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private SettleObligationCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new SettleObligationCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private SettlementObligation ExistingObligation(Guid obligationId, ObligationStatus status = ObligationStatus.Pending) => new()
    {
        ObligationId            = obligationId,
        TenantId                = TenantId,
        BrokerId                = Guid.NewGuid(),
        ClientId                = Guid.NewGuid(),
        BatchId                 = Guid.NewGuid(),
        SettlementNo            = "SN20240101",
        Status                  = status,
        FundsPayIn              = 10000m,
        FundsPayOut             = 0m,
        NetFundsObligation      = 10000m,
        SecuritiesPayIn         = 100,
        SecuritiesPayOut        = 0,
        NetSecuritiesObligation = 100
    };

    [Fact]
    public async Task Handle_WhenObligationNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new SettleObligationCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadySettled_ShouldThrowInvalidOperationException()
    {
        var obligationId = Guid.NewGuid();
        var obligation   = ExistingObligation(obligationId, ObligationStatus.Settled);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation> { obligation });

        var handler = CreateHandler();
        var act     = () => handler.Handle(new SettleObligationCommand(obligationId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToSettled()
    {
        var obligationId = Guid.NewGuid();
        var obligation   = ExistingObligation(obligationId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation> { obligation });

        var handler = CreateHandler();
        var result  = await handler.Handle(new SettleObligationCommand(obligationId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(ObligationStatus.Settled);
    }

    [Fact]
    public async Task Handle_ShouldSetSettledAt()
    {
        var obligationId = Guid.NewGuid();
        var obligation   = ExistingObligation(obligationId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation> { obligation });

        var handler = CreateHandler();
        await handler.Handle(new SettleObligationCommand(obligationId), CancellationToken.None);

        obligation.SettledAt.Should().NotBeNull();
        obligation.SettledAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSave()
    {
        var obligationId = Guid.NewGuid();
        var obligation   = ExistingObligation(obligationId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<SettlementObligation> { obligation });

        var handler = CreateHandler();
        await handler.Handle(new SettleObligationCommand(obligationId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(obligation, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
