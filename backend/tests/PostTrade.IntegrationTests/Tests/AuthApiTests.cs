namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class AuthApiTests : BaseIntegrationTest
{
    public AuthApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [SkippableFact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "Admin@123",
            TenantCode = "DEMO"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();

        var token = body.GetProperty("data").GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty();
    }

    [SkippableFact]
    public async Task Login_WrongPassword_ReturnsFailure()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "WrongPassword!",
            TenantCode = "DEMO"
        });

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [SkippableFact]
    public async Task Login_WrongTenantCode_ReturnsFailure()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "Admin@123",
            TenantCode = "NONEXISTENT"
        });

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [SkippableFact]
    public async Task Login_ValidCredentials_ResponseContainsUsername()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "Admin@123",
            TenantCode = "DEMO"
        });

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("username").GetString()
            .Should().Be("admin");
    }
}
