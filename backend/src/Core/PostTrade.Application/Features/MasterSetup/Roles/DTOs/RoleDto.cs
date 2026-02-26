namespace PostTrade.Application.Features.MasterSetup.Roles.DTOs;

public record RoleDto(
    Guid RoleId,
    Guid TenantId,
    string RoleName,
    string? Description,
    bool IsSystemRole
);
