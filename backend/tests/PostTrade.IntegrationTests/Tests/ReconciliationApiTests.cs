namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class ReconciliationApiTests : BaseIntegrationTest, IAsyncLifetime
{
    public ReconciliationApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync() => await AuthenticateAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task RunReconciliation_MatchedValues_Returns201WithMatchedStatus()
    {
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var response = await Client.PostAsJsonAsync("/api/reconciliation/run", new
        {
            ReconDate = DateTime.UtcNow.Date,
            SettlementNo = $"REC{unique}",
            ReconType = 1,          // Trade
            SystemValue = 1000000m,
            ExchangeValue = 1000000m,
            ToleranceLimit = 0.01m,
            Comments = "Integration test reconciliation - matched"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();

        var data = body.GetProperty("data");
        data.GetProperty("reconId").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("status").GetString().Should().Be("Matched");
    }

    [SkippableFact]
    public async Task RunReconciliation_MismatchedValues_Returns201WithMismatchedStatus()
    {
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var response = await Client.PostAsJsonAsync("/api/reconciliation/run", new
        {
            ReconDate = DateTime.UtcNow.Date,
            SettlementNo = $"RECM{unique}",
            ReconType = 1,          // Trade
            SystemValue = 1000000m,
            ExchangeValue = 999000m,  // 1000 difference > tolerance
            ToleranceLimit = 0.01m,
            Comments = "Integration test reconciliation - mismatched"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = body.GetProperty("data");
        data.GetProperty("status").GetString().Should().Be("Mismatched");
    }

    [SkippableFact]
    public async Task GetReconciliationExceptions_Returns200WithList()
    {
        // First create a mismatched recon so there is at least one exception
        var unique = Guid.NewGuid().ToString("N")[..8].ToUpper();
        await Client.PostAsJsonAsync("/api/reconciliation/run", new
        {
            ReconDate = DateTime.UtcNow.Date,
            SettlementNo = $"RECE{unique}",
            ReconType = 2,          // Position
            SystemValue = 500000m,
            ExchangeValue = 480000m,
            ToleranceLimit = 1m,
            Comments = "Exception test"
        });

        var response = await Client.GetAsync("/api/reconciliation/exceptions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);

        // At least one exception should exist (from the mismatched recon above)
        var exceptions = body.GetProperty("data").EnumerateArray().ToList();
        exceptions.Should().NotBeEmpty();
    }
}
