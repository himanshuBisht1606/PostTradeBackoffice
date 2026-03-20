using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Brokers.Commands;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Features.MasterSetup.Brokers.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.MasterSetup;

public static class BrokerEndpoints
{
    public static RouteGroupBuilder MapBrokerEndpoints(this RouteGroupBuilder group)
    {
        // GET /api/brokers — list (summary)
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBrokersQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<BrokerDto>>.Ok(result));
        }).WithTags("Brokers");

        // GET /api/brokers/{id} — full detail + exchange memberships
        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBrokerByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<BrokerDetailDto>.Fail("Broker not found"))
                : Results.Ok(ApiResponse<BrokerDetailDto>.Ok(result));
        }).WithTags("Brokers");

        // POST /api/brokers — create
        group.MapPost("/", async (CreateBrokerCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/brokers/{result.BrokerId}", ApiResponse<BrokerDto>.Ok(result, "Broker created"));
        }).WithTags("Brokers");

        // PUT /api/brokers/{id} — update full profile
        group.MapPut("/{id:guid}", async (Guid id, UpdateBrokerCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { BrokerId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<BrokerDto>.Fail("Broker not found"))
                : Results.Ok(ApiResponse<BrokerDto>.Ok(result, "Broker updated"));
        }).WithTags("Brokers");

        // PATCH /api/brokers/{id}/status — quick status change
        group.MapPatch("/{id:guid}/status", async (Guid id, ChangeBrokerStatusRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ChangeBrokerStatusCommand(id, req.Status), ct);
            return result
                ? Results.Ok(ApiResponse<string>.Ok("Status updated"))
                : Results.NotFound(ApiResponse<string>.Fail("Broker not found"));
        }).WithTags("Brokers");

        // PUT /api/brokers/{id}/memberships — upsert exchange membership
        group.MapPut("/{id:guid}/memberships", async (
            Guid id,
            UpsertBrokerExchangeMembershipCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = command with { BrokerId = id };
            var result = await sender.Send(cmd, ct);
            return Results.Ok(ApiResponse<BrokerExchangeMembershipDto>.Ok(result, "Membership saved"));
        }).WithTags("Brokers");

        // DELETE /api/brokers/{id}/memberships/{membershipId}
        group.MapDelete("/{id:guid}/memberships/{membershipId:guid}", async (
            Guid id,
            Guid membershipId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteBrokerExchangeMembershipCommand(id, membershipId), ct);
            return result
                ? Results.Ok(ApiResponse<string>.Ok("Membership removed"))
                : Results.NotFound(ApiResponse<string>.Fail("Membership not found"));
        }).WithTags("Brokers");

        return group;
    }
}

internal record ChangeBrokerStatusRequest(BrokerStatus Status);
