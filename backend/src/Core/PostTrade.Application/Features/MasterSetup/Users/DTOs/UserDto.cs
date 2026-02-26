using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Users.DTOs;

public record UserDto(
    Guid UserId,
    Guid TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    UserStatus Status,
    DateTime? LastLoginAt,
    IEnumerable<string> Roles
);
