using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Clients.Commands;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Features.MasterSetup.Clients.Queries;
using PostTrade.Domain.Enums;

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
                ? Results.NotFound(ApiResponse<ClientDetailDto>.Fail("Client not found"))
                : Results.Ok(ApiResponse<ClientDetailDto>.Ok(result));
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
                ? Results.NotFound(ApiResponse<ClientDetailDto>.Fail("Client not found"))
                : Results.Ok(ApiResponse<ClientDetailDto>.Ok(result, "Client updated"));
        }).WithTags("Clients");

        // PATCH /api/clients/{id}/status — quick status change
        group.MapPatch("/{id:guid}/status", async (Guid id, ChangeStatusRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ChangeClientStatusCommand(id, req.Status), ct);
            return result
                ? Results.Ok(ApiResponse<string>.Ok("Status updated"))
                : Results.NotFound(ApiResponse<string>.Fail("Client not found"));
        }).WithTags("Clients");

        // POST /api/clients/onboard — multi-step onboarding wizard
        group.MapPost("/onboard", async (OnboardClientCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created(
                $"/api/clients/{result.ClientId}",
                ApiResponse<OnboardingResultDto>.Ok(result, "Client onboarded successfully"));
        }).WithTags("Clients");

        return group;
    }
}

internal record ChangeStatusRequest(ClientStatus Status);
