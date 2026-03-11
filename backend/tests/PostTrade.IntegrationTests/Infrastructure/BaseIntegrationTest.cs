using System.Net.Http.Headers;

namespace PostTrade.IntegrationTests.Infrastructure;

/// <summary>
/// Abstract base for all integration test classes.
/// Provides an authenticated HttpClient and a helper to obtain a bearer token
/// using the seeded DEMO/admin/Admin@123 credentials.
/// </summary>
public abstract class BaseIntegrationTest
{
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        if (!factory.DockerAvailable)
            throw new SkipException("Docker is not running or not installed. Integration tests require Docker.");

        Client = factory.CreateClient();
    }

    /// <summary>
    /// Authenticates as the seeded admin user and attaches the bearer token to
    /// <see cref="Client"/>. Call this at the start of any test that needs
    /// authorization, or once in <c>InitializeAsync()</c>.
    /// </summary>
    protected async Task AuthenticateAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "Admin@123",
            TenantCode = "DEMO"
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("data").GetProperty("token").GetString()!;

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Extracts the strongly-typed <c>data</c> payload from an ApiResponse envelope.
    /// </summary>
    protected static T? GetData<T>(JsonElement root)
    {
        var dataElement = root.GetProperty("data");
        return dataElement.Deserialize<T>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
