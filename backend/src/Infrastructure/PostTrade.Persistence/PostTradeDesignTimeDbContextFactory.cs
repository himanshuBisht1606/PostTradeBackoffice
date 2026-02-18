using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PostTrade.Application.Interfaces;
using PostTrade.Persistence.Context;

namespace PostTrade.Persistence;

/// <summary>
/// Used by EF Core design-time tools (migrations) to create PostTradeDbContext
/// without relying on the DI container.
/// </summary>
public class PostTradeDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostTradeDbContext>
{
    public PostTradeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
                "../../Presentation/PostTrade.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<PostTradeDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsAssembly("PostTrade.Persistence"));

        return new PostTradeDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    /// <summary>
    /// No-op tenant context used only at design-time. GetCurrentTenantId returns Guid.Empty
    /// which causes OnModelCreating to skip query filters safely.
    /// </summary>
    private class DesignTimeTenantContext : ITenantContext
    {
        public Guid GetCurrentTenantId() => Guid.Empty;
        public void SetTenantId(Guid tenantId) { }
    }
}
