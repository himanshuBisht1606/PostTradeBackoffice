using PostTrade.Application.Features.Ledger.Entries.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Ledger;

public class GetLedgerEntriesQueryHandlerTests
{
    private readonly Mock<IRepository<LedgerEntry>> _repo          = new();
    private readonly Mock<ITenantContext>           _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ClientId = Guid.NewGuid();

    private GetLedgerEntriesQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetLedgerEntriesQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static LedgerEntry MakeEntry(int daysOffset = 0) => new()
    {
        LedgerId      = Guid.NewGuid(),
        TenantId      = TenantId,
        BrokerId      = Guid.NewGuid(),
        ClientId      = ClientId,
        VoucherNo     = "VCH" + Guid.NewGuid().ToString("N")[..6],
        PostingDate   = DateTime.Today.AddDays(-daysOffset),
        ValueDate     = DateTime.Today.AddDays(-daysOffset),
        LedgerType    = LedgerType.ClientLedger,
        EntryType     = EntryType.Trade,
        Debit         = 0m,
        Credit        = 5000m,
        Balance       = 5000m,
        ReferenceType = "Trade",
        ReferenceId   = Guid.NewGuid(),
        CreatedAt     = DateTime.UtcNow.AddDays(-daysOffset)
    };

    [Fact]
    public async Task Handle_ShouldReturnAllEntriesForTenant()
    {
        var entries = new List<LedgerEntry> { MakeEntry(0), MakeEntry(1) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entries);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetLedgerEntriesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoEntries_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<LedgerEntry>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetLedgerEntriesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEntriesOrderedByPostingDateDescending()
    {
        var older  = MakeEntry(2);
        var newer  = MakeEntry(0);
        var middle = MakeEntry(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<LedgerEntry> { older, newer, middle });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetLedgerEntriesQuery(), CancellationToken.None)).ToList();

        result[0].PostingDate.Should().BeOnOrAfter(result[1].PostingDate);
        result[1].PostingDate.Should().BeOnOrAfter(result[2].PostingDate);
    }

    [Fact]
    public async Task Handle_WhenFilteringByClientId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<LedgerEntry>());

        var handler = CreateHandler();
        await handler.Handle(new GetLedgerEntriesQuery(ClientId: ClientId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LedgerEntry, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<LedgerEntry>());

        var handler = CreateHandler();
        await handler.Handle(new GetLedgerEntriesQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
