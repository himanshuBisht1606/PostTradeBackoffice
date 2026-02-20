using PostTrade.Application.Features.Settlement.Batches.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Settlement;

public class ProcessSettlementBatchCommandHandlerTests
{
    private readonly Mock<IRepository<SettlementBatch>>      _batchRepo      = new();
    private readonly Mock<IRepository<SettlementObligation>> _obligationRepo = new();
    private readonly Mock<IUnitOfWork>                       _unitOfWork     = new();
    private readonly Mock<ITenantContext>                    _tenantContext  = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private ProcessSettlementBatchCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _obligationRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementObligation>());
        return new ProcessSettlementBatchCommandHandler(
            _batchRepo.Object, _obligationRepo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private SettlementBatch ExistingBatch(Guid batchId, SettlementStatus status = SettlementStatus.Pending) => new()
    {
        BatchId        = batchId,
        TenantId       = TenantId,
        SettlementNo   = "SN20240101",
        TradeDate      = DateTime.Today,
        SettlementDate = DateTime.Today.AddDays(2),
        ExchangeId     = Guid.NewGuid(),
        TotalTrades    = 5,
        TotalTurnover  = 250000m,
        Status         = status
    };

    private static SettlementObligation PendingObligation(Guid batchId) => new()
    {
        ObligationId            = Guid.NewGuid(),
        TenantId                = TenantId,
        BrokerId                = Guid.NewGuid(),
        ClientId                = Guid.NewGuid(),
        BatchId                 = batchId,
        SettlementNo            = "SN20240101",
        Status                  = ObligationStatus.Pending,
        FundsPayIn              = 10000m,
        FundsPayOut             = 0m,
        NetFundsObligation      = 10000m,
        SecuritiesPayIn         = 100,
        SecuritiesPayOut        = 0,
        NetSecuritiesObligation = 100
    };

    [Fact]
    public async Task Handle_WhenBatchNotFound_ShouldReturnNull()
    {
        _batchRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementBatch>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new ProcessSettlementBatchCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(SettlementStatus.Processing)]
    [InlineData(SettlementStatus.Completed)]
    [InlineData(SettlementStatus.Failed)]
    public async Task Handle_WhenBatchIsNotPending_ShouldThrowInvalidOperationException(SettlementStatus status)
    {
        var batchId = Guid.NewGuid();
        var batch   = ExistingBatch(batchId, status);

        _batchRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementBatch> { batch });

        var handler = CreateHandler();
        var act     = () => handler.Handle(new ProcessSettlementBatchCommand(batchId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldSettleAllPendingObligationsInBatch()
    {
        var batchId     = Guid.NewGuid();
        var batch       = ExistingBatch(batchId);
        var obligation1 = PendingObligation(batchId);
        var obligation2 = PendingObligation(batchId);

        var handler = CreateHandler();

        _batchRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementBatch> { batch });
        _obligationRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementObligation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementObligation> { obligation1, obligation2 });

        await handler.Handle(new ProcessSettlementBatchCommand(batchId), CancellationToken.None);

        _obligationRepo.Verify(r => r.UpdateAsync(It.IsAny<SettlementObligation>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        obligation1.Status.Should().Be(ObligationStatus.Settled);
        obligation2.Status.Should().Be(ObligationStatus.Settled);
    }

    [Fact]
    public async Task Handle_ShouldTransitionBatchStatusToCompleted()
    {
        var batchId = Guid.NewGuid();
        var batch   = ExistingBatch(batchId);

        _batchRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementBatch> { batch });

        var handler = CreateHandler();
        var result  = await handler.Handle(new ProcessSettlementBatchCommand(batchId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SettlementStatus.Completed);
    }

    [Fact]
    public async Task Handle_ShouldSetProcessedAt()
    {
        var batchId = Guid.NewGuid();
        var batch   = ExistingBatch(batchId);

        _batchRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<SettlementBatch, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SettlementBatch> { batch });

        var handler = CreateHandler();
        await handler.Handle(new ProcessSettlementBatchCommand(batchId), CancellationToken.None);

        batch.ProcessedAt.Should().NotBeNull();
        batch.ProcessedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
