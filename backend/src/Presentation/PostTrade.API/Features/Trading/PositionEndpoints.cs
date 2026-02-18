using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Trading.Positions.DTOs;
using PostTrade.Application.Features.Trading.Positions.Queries;

namespace PostTrade.API.Features.Trading;

public static class PositionEndpoints
{
    public static RouteGroupBuilder MapPositionEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPositionsQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<PositionDto>>.Ok(result));
        }).WithTags("Positions");

        group.MapGet("/client/{clientId:guid}", async (Guid clientId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPositionsByClientQuery(clientId), ct);
            return Results.Ok(ApiResponse<IEnumerable<PositionDto>>.Ok(result));
        }).WithTags("Positions");

        return group;
    }
}
