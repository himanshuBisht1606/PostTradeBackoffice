namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class SettlementApiTests : BaseIntegrationTest, IAsyncLifetime
{
    private Guid _exchangeId;

    public SettlementApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync()
    {
        await AuthenticateAsync();

        // Create an exchange to use as a prerequisite
        var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();
        var response = await Client.PostAsJsonAsync("/api/exchanges", new
        {
            ExchangeCode = $"SEX{unique}",
            ExchangeName = $"Settlement Exchange {unique}",
            Country = "India"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        _exchangeId = Guid.Parse(body.GetProperty("data").GetProperty("exchangeId").GetString()!);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSettlementBatches_Returns200WithList()
    {
        var response = await Client.GetAsync("/api/settlement/batches");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task CreateSettlementBatch_ValidPayload_Returns201()
    {
        var tradeDate = DateTime.UtcNow.Date;
        var settlementDate = tradeDate.AddDays(2);
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var response = await Client.PostAsJsonAsync("/api/settlement/batches", new
        {
            SettlementNo = $"SB{unique}",
            TradeDate = tradeDate,
            SettlementDate = settlementDate,
            ExchangeId = _exchangeId,
            TotalTrades = 10,
            TotalTurnover = 500000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("batchId").GetString()
            .Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessSettlementBatch_ExistingBatch_Returns200()
    {
        // Create a batch first
        var tradeDate = DateTime.UtcNow.Date;
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var createResponse = await Client.PostAsJsonAsync("/api/settlement/batches", new
        {
            SettlementNo = $"SBP{unique}",
            TradeDate = tradeDate,
            SettlementDate = tradeDate.AddDays(2),
            ExchangeId = _exchangeId,
            TotalTrades = 5,
            TotalTurnover = 250000m
        });
        createResponse.EnsureSuccessStatusCode();
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var batchId = createBody.GetProperty("data").GetProperty("batchId").GetString();

        // Process it
        var response = await Client.PutAsync($"/api/settlement/batches/{batchId}/process", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("status").GetString()
            .Should().Be("Processed");
    }

    [Fact]
    public async Task ProcessSettlementBatch_NonExistentId_Returns404()
    {
        var response = await Client.PutAsync($"/api/settlement/batches/{Guid.NewGuid()}/process", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
