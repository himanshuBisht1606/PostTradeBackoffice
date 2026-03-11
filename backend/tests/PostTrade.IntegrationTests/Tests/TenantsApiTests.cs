namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class TenantsApiTests : BaseIntegrationTest, IAsyncLifetime
{
    public TenantsApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync() => await AuthenticateAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task GetTenants_Returns200WithSeededTenant()
    {
        var response = await Client.GetAsync("/api/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();

        var items = body.GetProperty("data").EnumerateArray().ToList();
        items.Should().NotBeEmpty();
        items.Should().Contain(t =>
            t.GetProperty("tenantCode").GetString() == "DEMO");
    }

    [SkippableFact]
    public async Task CreateTenant_ValidPayload_Returns201WithNewTenant()
    {
        var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();
        var response = await Client.PostAsJsonAsync("/api/tenants", new
        {
            TenantCode = $"T{unique}",
            TenantName = $"Test Tenant {unique}",
            ContactEmail = $"test{unique}@example.com",
            ContactPhone = "+91-9999999999"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("tenantCode").GetString()
            .Should().Be($"T{unique}");
    }

    [SkippableFact]
    public async Task GetTenantById_SeededTenant_Returns200()
    {
        // First get all tenants to find the seeded DEMO tenant's ID
        var listResponse = await Client.GetAsync("/api/tenants");
        var listBody = await listResponse.Content.ReadFromJsonAsync<JsonElement>();

        var demoTenant = listBody.GetProperty("data").EnumerateArray()
            .First(t => t.GetProperty("tenantCode").GetString() == "DEMO");
        var tenantId = demoTenant.GetProperty("tenantId").GetString();

        var response = await Client.GetAsync($"/api/tenants/{tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("tenantCode").GetString()
            .Should().Be("DEMO");
    }

    [SkippableFact]
    public async Task GetTenantById_NonExistentId_Returns404()
    {
        var response = await Client.GetAsync($"/api/tenants/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
