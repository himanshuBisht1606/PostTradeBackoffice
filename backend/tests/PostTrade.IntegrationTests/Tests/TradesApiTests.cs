namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class TradesApiTests : BaseIntegrationTest, IAsyncLifetime
{
    private Guid _brokerId;
    private Guid _clientId;
    private Guid _instrumentId;

    public TradesApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync()
    {
        await AuthenticateAsync();
        await SetupTestDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SetupTestDataAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Exchange
        var exchangeResponse = await Client.PostAsJsonAsync("/api/exchanges", new
        {
            ExchangeCode = $"EX{unique}",
            ExchangeName = $"Test Exchange {unique}",
            Country = "India"
        });
        exchangeResponse.EnsureSuccessStatusCode();
        var exchangeBody = await exchangeResponse.Content.ReadFromJsonAsync<JsonElement>();
        var exchangeId = Guid.Parse(exchangeBody.GetProperty("data").GetProperty("exchangeId").GetString()!);

        // Segment
        var segmentResponse = await Client.PostAsJsonAsync("/api/segments", new
        {
            ExchangeId = exchangeId,
            SegmentCode = $"SEG{unique}",
            SegmentName = $"Test Segment {unique}"
        });
        segmentResponse.EnsureSuccessStatusCode();
        var segmentBody = await segmentResponse.Content.ReadFromJsonAsync<JsonElement>();
        var segmentId = Guid.Parse(segmentBody.GetProperty("data").GetProperty("segmentId").GetString()!);

        // Instrument
        var instrResponse = await Client.PostAsJsonAsync("/api/instruments", new
        {
            InstrumentCode = $"INST{unique}",
            InstrumentName = $"Test Instrument {unique}",
            Symbol = $"TST{unique}",
            ExchangeId = exchangeId,
            SegmentId = segmentId,
            InstrumentType = 1, // Equity
            LotSize = 1m,
            TickSize = 0.05m
        });
        instrResponse.EnsureSuccessStatusCode();
        var instrBody = await instrResponse.Content.ReadFromJsonAsync<JsonElement>();
        _instrumentId = Guid.Parse(instrBody.GetProperty("data").GetProperty("instrumentId").GetString()!);

        // Broker
        var brokerResponse = await Client.PostAsJsonAsync("/api/brokers", new
        {
            BrokerCode = $"BR{unique}",
            BrokerName = $"Test Broker {unique}",
            ContactEmail = $"broker{unique}@example.com",
            ContactPhone = "+91-9000000003"
        });
        brokerResponse.EnsureSuccessStatusCode();
        var brokerBody = await brokerResponse.Content.ReadFromJsonAsync<JsonElement>();
        _brokerId = Guid.Parse(brokerBody.GetProperty("data").GetProperty("brokerId").GetString()!);

        // Client
        var clientResponse = await Client.PostAsJsonAsync("/api/clients", new
        {
            BrokerId = _brokerId,
            ClientCode = $"CLI{unique}",
            ClientName = $"Trade Client {unique}",
            Email = $"tradeclient{unique}@example.com",
            Phone = "+91-9000000004",
            ClientType = 1  // Individual
        });
        clientResponse.EnsureSuccessStatusCode();
        var clientBody = await clientResponse.Content.ReadFromJsonAsync<JsonElement>();
        _clientId = Guid.Parse(clientBody.GetProperty("data").GetProperty("clientId").GetString()!);
    }

    [Fact]
    public async Task GetTrades_Returns200WithList()
    {
        var response = await Client.GetAsync("/api/trades");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task BookTrade_ValidPayload_Returns201WithTrade()
    {
        var response = await Client.PostAsJsonAsync("/api/trades", new
        {
            BrokerId = _brokerId,
            ClientId = _clientId,
            InstrumentId = _instrumentId,
            Side = 1,                              // Buy
            Quantity = 100,
            Price = 500.50m,
            TradeDate = DateTime.UtcNow.Date,
            SettlementNo = $"SNO{Guid.NewGuid():N}",
            Source = 3                             // Manual
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();

        var data = body.GetProperty("data");
        data.GetProperty("tradeId").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("status").GetString().Should().Be("Pending");
    }

    [Fact]
    public async Task GetTradeById_ExistingTrade_Returns200()
    {
        // Book a trade first
        var bookResponse = await Client.PostAsJsonAsync("/api/trades", new
        {
            BrokerId = _brokerId,
            ClientId = _clientId,
            InstrumentId = _instrumentId,
            Side = 1,
            Quantity = 10,
            Price = 100m,
            TradeDate = DateTime.UtcNow.Date,
            SettlementNo = $"SNO{Guid.NewGuid():N}",
            Source = 3
        });
        bookResponse.EnsureSuccessStatusCode();
        var bookBody = await bookResponse.Content.ReadFromJsonAsync<JsonElement>();
        var tradeId = bookBody.GetProperty("data").GetProperty("tradeId").GetString();

        // Now fetch it
        var response = await Client.GetAsync($"/api/trades/{tradeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("tradeId").GetString().Should().Be(tradeId);
    }

    [Fact]
    public async Task GetTradeById_NonExistentId_Returns404()
    {
        var response = await Client.GetAsync($"/api/trades/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelTrade_PendingTrade_Returns200WithCancelledStatus()
    {
        // Book a trade first
        var bookResponse = await Client.PostAsJsonAsync("/api/trades", new
        {
            BrokerId = _brokerId,
            ClientId = _clientId,
            InstrumentId = _instrumentId,
            Side = 2,       // Sell
            Quantity = 50,
            Price = 250m,
            TradeDate = DateTime.UtcNow.Date,
            SettlementNo = $"SNO{Guid.NewGuid():N}",
            Source = 3
        });
        bookResponse.EnsureSuccessStatusCode();
        var bookBody = await bookResponse.Content.ReadFromJsonAsync<JsonElement>();
        var tradeId = bookBody.GetProperty("data").GetProperty("tradeId").GetString();

        // Cancel it
        var response = await Client.PutAsJsonAsync($"/api/trades/{tradeId}/cancel", new
        {
            Reason = "Integration test cancellation"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("status").GetString().Should().Be("Cancelled");
    }
}
