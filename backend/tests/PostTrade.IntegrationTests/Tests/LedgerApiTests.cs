namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class LedgerApiTests : BaseIntegrationTest, IAsyncLifetime
{
    private Guid _brokerId;
    private Guid _clientId;

    public LedgerApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync()
    {
        await AuthenticateAsync();
        await SetupTestDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SetupTestDataAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Broker
        var brokerResponse = await Client.PostAsJsonAsync("/api/brokers", new
        {
            BrokerCode = $"LBR{unique}",
            BrokerName = $"Ledger Broker {unique}",
            ContactEmail = $"ledger.broker{unique}@example.com",
            ContactPhone = "+91-9000000005"
        });
        brokerResponse.EnsureSuccessStatusCode();
        var brokerBody = await brokerResponse.Content.ReadFromJsonAsync<JsonElement>();
        _brokerId = Guid.Parse(brokerBody.GetProperty("data").GetProperty("brokerId").GetString()!);

        // Client
        var clientResponse = await Client.PostAsJsonAsync("/api/clients", new
        {
            BrokerId = _brokerId,
            ClientCode = $"LCLI{unique}",
            ClientName = $"Ledger Client {unique}",
            Email = $"ledger.client{unique}@example.com",
            Phone = "+91-9000000006",
            ClientType = 1  // Individual
        });
        clientResponse.EnsureSuccessStatusCode();
        var clientBody = await clientResponse.Content.ReadFromJsonAsync<JsonElement>();
        _clientId = Guid.Parse(clientBody.GetProperty("data").GetProperty("clientId").GetString()!);
    }

    [Fact]
    public async Task GetLedgerEntries_Returns200WithList()
    {
        var response = await Client.GetAsync("/api/ledger/entries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task PostLedgerEntry_ValidCreditEntry_Returns201()
    {
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var response = await Client.PostAsJsonAsync("/api/ledger/entries", new
        {
            BrokerId = _brokerId,
            ClientId = _clientId,
            VoucherNo = $"VCH{unique}",
            PostingDate = DateTime.UtcNow.Date,
            ValueDate = DateTime.UtcNow.Date,
            LedgerType = 1,     // ClientLedger
            EntryType = 3,      // Payment
            Debit = 0m,
            Credit = 100000m,
            ReferenceType = "Payment",
            ReferenceId = Guid.NewGuid(),
            Narration = "Integration test credit entry"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("ledgerId").GetString()
            .Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostLedgerEntry_ValidDebitEntry_Returns201()
    {
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();

        // Credit first so balance is positive
        await Client.PostAsJsonAsync("/api/ledger/entries", new
        {
            BrokerId = _brokerId,
            ClientId = _clientId,
            VoucherNo = $"VCH{unique}A",
            PostingDate = DateTime.UtcNow.Date,
            ValueDate = DateTime.UtcNow.Date,
            LedgerType = 1,
            EntryType = 3,
            Debit = 0m,
            Credit = 200000m,
            ReferenceType = "Payment",
            ReferenceId = Guid.NewGuid()
        });

        var response = await Client.PostAsJsonAsync("/api/ledger/entries", new
        {
            BrokerId = _brokerId,
            ClientId = _clientId,
            VoucherNo = $"VCH{unique}B",
            PostingDate = DateTime.UtcNow.Date,
            ValueDate = DateTime.UtcNow.Date,
            LedgerType = 1,
            EntryType = 1,      // Trade
            Debit = 50000m,
            Credit = 0m,
            ReferenceType = "Trade",
            ReferenceId = Guid.NewGuid(),
            Narration = "Integration test debit entry"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
