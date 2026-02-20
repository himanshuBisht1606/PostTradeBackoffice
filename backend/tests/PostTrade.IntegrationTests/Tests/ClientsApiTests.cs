namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class ClientsApiTests : BaseIntegrationTest, IAsyncLifetime
{
    private Guid _brokerId;

    public ClientsApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync()
    {
        await AuthenticateAsync();

        // Create a broker to use as a prerequisite
        var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();
        var brokerResponse = await Client.PostAsJsonAsync("/api/brokers", new
        {
            BrokerCode = $"BR{unique}",
            BrokerName = $"Test Broker {unique}",
            ContactEmail = $"broker{unique}@example.com",
            ContactPhone = "+91-9000000001"
        });

        brokerResponse.EnsureSuccessStatusCode();
        var body = await brokerResponse.Content.ReadFromJsonAsync<JsonElement>();
        _brokerId = Guid.Parse(body.GetProperty("data").GetProperty("brokerId").GetString()!);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetClients_Returns200WithList()
    {
        var response = await Client.GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task CreateClient_ValidPayload_Returns201WithClient()
    {
        var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();
        var response = await Client.PostAsJsonAsync("/api/clients", new
        {
            BrokerId = _brokerId,
            ClientCode = $"CLI{unique}",
            ClientName = $"Test Client {unique}",
            Email = $"client{unique}@example.com",
            Phone = "+91-9000000002",
            ClientType = 1  // Individual
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("clientCode").GetString()
            .Should().Be($"CLI{unique}");
    }

    [Fact]
    public async Task CreateClient_WithoutAuth_Returns401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/clients", new
        {
            BrokerId = _brokerId,
            ClientCode = "UNAUTH",
            ClientName = "Unauthorized Client",
            Email = "unauth@example.com",
            Phone = "+91-9000000099",
            ClientType = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Re-authenticate for subsequent tests
        await AuthenticateAsync();
    }
}
