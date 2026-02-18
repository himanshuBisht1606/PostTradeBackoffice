using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Clients.Commands;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Features.MasterSetup.Clients.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class ClientEndpoints
{
    public static RouteGroupBuilder MapClientEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct, int page = 1, int pageSize = 20) =>
        {
            var result = await sender.Send(new GetClientsQuery(page, pageSize), ct);
            return Results.Ok(ApiResponse<IEnumerable<ClientDto>>.Ok(result));
        }).WithTags("Clients");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetClientByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<ClientDto>.Fail("Client not found"))
                : Results.Ok(ApiResponse<ClientDto>.Ok(result));
        }).WithTags("Clients");

        group.MapPost("/", async (CreateClientCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/clients/{result.ClientId}", ApiResponse<ClientDto>.Ok(result, "Client created"));
        }).WithTags("Clients");

        group.MapPut("/{id:guid}", async (Guid id, UpdateClientCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { ClientId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<ClientDto>.Fail("Client not found"))
                : Results.Ok(ApiResponse<ClientDto>.Ok(result, "Client updated"));
        }).WithTags("Clients");

        return group;
    }
}
