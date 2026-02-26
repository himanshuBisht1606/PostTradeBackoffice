using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Auth.Commands;
using PostTrade.Application.Features.Auth.DTOs;

namespace PostTrade.API.Features.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful"));
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithTags("Auth");

        return group;
    }
}
