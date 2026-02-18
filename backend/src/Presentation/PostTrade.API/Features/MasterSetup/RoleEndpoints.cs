using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Roles.Commands;
using PostTrade.Application.Features.MasterSetup.Roles.DTOs;
using PostTrade.Application.Features.MasterSetup.Roles.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class RoleEndpoints
{
    public static RouteGroupBuilder MapRoleEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRolesQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(result));
        }).WithTags("Roles");

        group.MapPost("/", async (CreateRoleCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/roles/{result.RoleId}", ApiResponse<RoleDto>.Ok(result, "Role created"));
        }).WithTags("Roles");

        group.MapPost("/{id:guid}/permissions", async (Guid id, AssignPermissionsCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { RoleId = id };
            var result = await sender.Send(cmd, ct);
            return result
                ? Results.Ok(ApiResponse<bool>.Ok(true, "Permissions assigned"))
                : Results.NotFound(ApiResponse<bool>.Fail("Role not found"));
        }).WithTags("Roles");

        return group;
    }
}
