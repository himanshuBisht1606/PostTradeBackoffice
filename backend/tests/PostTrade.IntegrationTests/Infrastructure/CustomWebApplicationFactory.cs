using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace PostTrade.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _dbContainer;

    /// <summary>
    /// False when Docker is not running/installed on this machine.
    /// Tests check this flag via <see cref="BaseIntegrationTest"/> and skip themselves.
    /// </summary>
    public bool DockerAvailable { get; private set; } = true;

    public string GetConnectionString() => _dbContainer?.GetConnectionString() ?? string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("posttrade_test")
                .WithUsername("postgres")
                .WithPassword("test_pass")
                .Build();

            await _dbContainer.StartAsync();
        }
        catch (ArgumentException)
        {
            // Docker is not running or not installed — tests will be skipped.
            DockerAvailable = false;
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_dbContainer is not null)
            await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        if (_dbContainer is not null)
            builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());

        // Keep JWT settings consistent with appsettings.json so tokens validate correctly
        builder.UseSetting("Jwt:Key", "your-256-bit-secret-key-here-minimum-32-chars!!");
        builder.UseSetting("Jwt:Issuer", "PostTradeBackoffice");
        builder.UseSetting("Jwt:Audience", "PostTradeBackoffice");
    }
}
