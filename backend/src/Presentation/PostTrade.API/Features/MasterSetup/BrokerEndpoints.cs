using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Brokers.Commands;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Features.MasterSetup.Brokers.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class BrokerEndpoints
{
    public static RouteGroupBuilder MapBrokerEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBrokersQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<BrokerDto>>.Ok(result));
        }).WithTags("Brokers");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBrokerByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<BrokerDto>.Fail("Broker not found"))
                : Results.Ok(ApiResponse<BrokerDto>.Ok(result));
        }).WithTags("Brokers");

        group.MapPost("/", async (CreateBrokerCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/brokers/{result.BrokerId}", ApiResponse<BrokerDto>.Ok(result, "Broker created"));
        }).WithTags("Brokers");

        group.MapPut("/{id:guid}", async (Guid id, UpdateBrokerCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { BrokerId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<BrokerDto>.Fail("Broker not found"))
                : Results.Ok(ApiResponse<BrokerDto>.Ok(result, "Broker updated"));
        }).WithTags("Brokers");

        return group;
    }
}
