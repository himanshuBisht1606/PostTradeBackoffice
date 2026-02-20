using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace PostTrade.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("posttrade_test")
        .WithUsername("postgres")
        .WithPassword("test_pass")
        .Build();

    public string GetConnectionString() => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the test PostgreSQL container instead of the configured DB
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());

        // Keep JWT settings consistent with appsettings.json so tokens validate correctly
        builder.UseSetting("Jwt:Key", "your-256-bit-secret-key-here-minimum-32-chars!!");
        builder.UseSetting("Jwt:Issuer", "PostTradeBackoffice");
        builder.UseSetting("Jwt:Audience", "PostTradeBackoffice");
    }
}
