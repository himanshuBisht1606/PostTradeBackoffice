using PostTrade.Application.Features.Ledger.Entries.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Ledger;

public class PostLedgerEntryCommandHandlerTests
{
    private readonly Mock<IRepository<LedgerEntry>> _repo          = new();
    private readonly Mock<IUnitOfWork>              _unitOfWork    = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid BrokerId    = Guid.NewGuid();
    private static readonly Guid ClientId    = Guid.NewGuid();
    private static readonly Guid ReferenceId = Guid.NewGuid();

    private PostLedgerEntryCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((LedgerEntry e, CancellationToken _) => e);
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<LedgerEntry>());
        return new PostLedgerEntryCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static PostLedgerEntryCommand ValidCommand(decimal debit = 0m, decimal credit = 5000m) => new(
        BrokerId:      BrokerId,
        ClientId:      ClientId,
        VoucherNo:     "VCH001",
        PostingDate:   DateTime.Today,
        ValueDate:     DateTime.Today,
        LedgerType:    LedgerType.ClientLedger,
        EntryType:     EntryType.Trade,
        Debit:         debit,
        Credit:        credit,
        ReferenceType: "Trade",
        ReferenceId:   ReferenceId,
        Narration:     "Test entry"
    );

    [Fact]
    public async Task Handle_WhenNoPreviousEntries_ShouldStartBalanceFromZero()
    {
        LedgerEntry? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()))
             .Callback<LedgerEntry, CancellationToken>((e, _) => captured = e)
             .ReturnsAsync((LedgerEntry e, CancellationToken _) => e);

        var command = ValidCommand(debit: 0m, credit: 5000m);
        await handler.Handle(command, CancellationToken.None);

        captured!.Balance.Should().Be(5000m); // 0 (lastBalance) + 5000 (credit) - 0 (debit)
    }

    [Fact]
    public async Task Handle_ShouldComputeBalanceAsLastBalancePlusCreditMinusDebit()
    {
        var existingEntry = new LedgerEntry
        {
            LedgerId    = Guid.NewGuid(),
            TenantId    = TenantId,
            ClientId    = ClientId,
            LedgerType  = LedgerType.ClientLedger,
            PostingDate = DateTime.Today.AddDays(-1),
            Balance     = 10000m,
            CreatedAt   = DateTime.UtcNow.AddDays(-1)
        };

        LedgerEntry? captured = null;
        var handler = CreateHandler();

        // Override defaults set inside CreateHandler() — last setup wins
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<LedgerEntry> { existingEntry });
        _repo.Setup(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()))
             .Callback<LedgerEntry, CancellationToken>((e, _) => captured = e)
             .ReturnsAsync((LedgerEntry e, CancellationToken _) => e);

        var command = ValidCommand(debit: 2000m, credit: 0m);
        await handler.Handle(command, CancellationToken.None);

        captured!.Balance.Should().Be(8000m); // 10000 + 0 - 2000
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        LedgerEntry? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()))
             .Callback<LedgerEntry, CancellationToken>((e, _) => captured = e)
             .ReturnsAsync((LedgerEntry e, CancellationToken _) => e);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
