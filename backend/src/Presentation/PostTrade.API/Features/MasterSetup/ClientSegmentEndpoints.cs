using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.ClientSegments.Commands;
using PostTrade.Application.Features.MasterSetup.ClientSegments.DTOs;
using PostTrade.Application.Features.MasterSetup.ClientSegments.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class ClientSegmentEndpoints
{
    public static RouteGroupBuilder MapClientSegmentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{clientId:guid}/segments", async (Guid clientId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetClientSegmentsQuery(clientId), ct);
            return Results.Ok(ApiResponse<IEnumerable<ClientSegmentActivationDto>>.Ok(result));
        }).WithTags("ClientSegments");

        group.MapPost("/{clientId:guid}/segments", async (Guid clientId, ActivateClientSegmentCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { ClientId = clientId };
            var result = await sender.Send(cmd, ct);
            return Results.Created($"/api/clients/{clientId}/segments/{result.ActivationId}",
                ApiResponse<ClientSegmentActivationDto>.Ok(result, "Segment activated for client"));
        }).WithTags("ClientSegments");

        group.MapPut("/segments/{activationId:guid}/deactivate", async (Guid activationId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeactivateClientSegmentCommand(activationId), ct);
            return result is null
                ? Results.NotFound(ApiResponse<ClientSegmentActivationDto>.Fail("Activation not found"))
                : Results.Ok(ApiResponse<ClientSegmentActivationDto>.Ok(result, "Segment deactivated"));
        }).WithTags("ClientSegments");

        return group;
    }
}
