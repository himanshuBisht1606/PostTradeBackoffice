using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Trading.PnL.DTOs;
using PostTrade.Application.Features.Trading.PnL.Queries;

namespace PostTrade.API.Features.Trading;

public static class PnLEndpoints
{
    public static RouteGroupBuilder MapPnLEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct, Guid? clientId = null) =>
        {
            var result = await sender.Send(new GetPnLSnapshotsQuery(clientId), ct);
            return Results.Ok(ApiResponse<IEnumerable<PnLSnapshotDto>>.Ok(result));
        }).WithTags("PnL");

        group.MapGet("/{date}", async (string date, ISender sender, CancellationToken ct, Guid? clientId = null) =>
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return Results.BadRequest(ApiResponse<IEnumerable<PnLSnapshotDto>>.Fail("Invalid date format. Use yyyy-MM-dd"));

            var result = await sender.Send(new GetPnLSnapshotByDateQuery(parsedDate, clientId), ct);
            return Results.Ok(ApiResponse<IEnumerable<PnLSnapshotDto>>.Ok(result));
        }).WithTags("PnL");

        return group;
    }
}
