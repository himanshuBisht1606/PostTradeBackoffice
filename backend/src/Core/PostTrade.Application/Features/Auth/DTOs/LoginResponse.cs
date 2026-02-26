namespace PostTrade.Application.Features.Auth.DTOs;

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string Username,
    IEnumerable<string> Roles
);
