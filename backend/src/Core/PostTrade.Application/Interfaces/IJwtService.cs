using System.Security.Claims;

namespace PostTrade.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, Guid tenantId, string username, IEnumerable<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
}
