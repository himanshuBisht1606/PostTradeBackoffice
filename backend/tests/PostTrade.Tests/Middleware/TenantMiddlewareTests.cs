using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PostTrade.API.Middleware;
using PostTrade.Application.Interfaces;

namespace PostTrade.Tests.Middleware;

public class TenantMiddlewareTests
{
    private static DefaultHttpContext CreateContextWithClaim(string claimType, string value)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(claimType, value) }, "test"));
        return context;
    }

    [Fact]
    public async Task Invoke_WhenTenantIdClaimPresent_ShouldSetTenantContext()
    {
        var tenantId      = Guid.NewGuid();
        var tenantContext = new Mock<ITenantContext>();
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware   = new TenantMiddleware(next);
        var context      = CreateContextWithClaim("TenantId", tenantId.ToString());

        await middleware.InvokeAsync(context, tenantContext.Object);

        tenantContext.Verify(t => t.SetTenantId(tenantId), Times.Once);
    }

    [Fact]
    public async Task Invoke_WhenNoTenantIdClaim_ShouldNotSetTenantContext()
    {
        var tenantContext = new Mock<ITenantContext>();
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware   = new TenantMiddleware(next);
        var context      = new DefaultHttpContext(); // no claims

        await middleware.InvokeAsync(context, tenantContext.Object);

        tenantContext.Verify(t => t.SetTenantId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Invoke_WhenTenantIdIsInvalidGuid_ShouldNotSetTenantContext()
    {
        var tenantContext = new Mock<ITenantContext>();
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware   = new TenantMiddleware(next);
        var context      = CreateContextWithClaim("TenantId", "not-a-valid-guid");

        await middleware.InvokeAsync(context, tenantContext.Object);

        tenantContext.Verify(t => t.SetTenantId(It.IsAny<Guid>()), Times.Never);
    }
}
