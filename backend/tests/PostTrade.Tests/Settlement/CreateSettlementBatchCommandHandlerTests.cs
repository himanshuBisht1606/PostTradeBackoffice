using PostTrade.Application.Features.Settlement.Batches.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Settlement;

public class CreateSettlementBatchCommandHandlerTests
{
    private readonly Mock<IRepository<SettlementBatch>> _repo          = new();
    private readonly Mock<IUnitOfWork>                  _unitOfWork    = new();
    private readonly Mock<ITenantContext>               _tenantContext = new();

    private static readonly Guid TenantId   = Guid.NewGuid();
    private static readonly Guid ExchangeId = Guid.NewGuid();

    private CreateSettlementBatchCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<SettlementBatch>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((SettlementBatch b, CancellationToken _) => b);
        return new CreateSettlementBatchCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateSettlementBatchCommand ValidCommand() => new(
        SettlementNo:   "SN20240101",
        TradeDate:      DateTime.Today,
        SettlementDate: DateTime.Today.AddDays(2),
        ExchangeId:     ExchangeId,
        TotalTrades:    10,
        TotalTurnover:  500000m
    );

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        SettlementBatch? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<SettlementBatch>(), It.IsAny<CancellationToken>()))
             .Callback<SettlementBatch, CancellationToken>((b, _) => captured = b)
             .ReturnsAsync((SettlementBatch b, CancellationToken _) => b);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToPending()
    {
        SettlementBatch? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<SettlementBatch>(), It.IsAny<CancellationToken>()))
             .Callback<SettlementBatch, CancellationToken>((b, _) => captured = b)
             .ReturnsAsync((SettlementBatch b, CancellationToken _) => b);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.Status.Should().Be(SettlementStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldGenerateBatchId()
    {
        SettlementBatch? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<SettlementBatch>(), It.IsAny<CancellationToken>()))
             .Callback<SettlementBatch, CancellationToken>((b, _) => captured = b)
             .ReturnsAsync((SettlementBatch b, CancellationToken _) => b);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.BatchId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<SettlementBatch>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.SettlementNo.Should().Be(command.SettlementNo);
        result.ExchangeId.Should().Be(command.ExchangeId);
        result.TotalTrades.Should().Be(command.TotalTrades);
        result.TotalTurnover.Should().Be(command.TotalTurnover);
        result.Status.Should().Be(SettlementStatus.Pending);
    }
}
