using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Users.Commands;
using PostTrade.Application.Features.MasterSetup.Users.DTOs;
using PostTrade.Application.Features.MasterSetup.Users.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result));
        }).WithTags("Users");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<UserDto>.Fail("User not found"))
                : Results.Ok(ApiResponse<UserDto>.Ok(result));
        }).WithTags("Users");

        group.MapPost("/", async (CreateUserCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/users/{result.UserId}", ApiResponse<UserDto>.Ok(result, "User created"));
        }).WithTags("Users");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { UserId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<UserDto>.Fail("User not found"))
                : Results.Ok(ApiResponse<UserDto>.Ok(result, "User updated"));
        }).WithTags("Users");

        group.MapPost("/{id:guid}/roles", async (Guid id, AssignRolesCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { UserId = id };
            var result = await sender.Send(cmd, ct);
            return result
                ? Results.Ok(ApiResponse<bool>.Ok(true, "Roles assigned"))
                : Results.NotFound(ApiResponse<bool>.Fail("User not found"));
        }).WithTags("Users");

        return group;
    }
}
