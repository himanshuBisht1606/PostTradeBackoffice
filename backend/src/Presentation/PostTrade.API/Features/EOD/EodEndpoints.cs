using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.EOD.Commands;
using PostTrade.Application.Features.EOD.DTOs;
using PostTrade.Application.Features.EOD.Queries;

namespace PostTrade.API.Features.EOD;

public static class EodEndpoints
{
    public static RouteGroupBuilder MapEodEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/run", async (RunEodCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.Success
                ? Results.Ok(ApiResponse<EodRunResultDto>.Ok(result, result.Message))
                : Results.Ok(ApiResponse<EodRunResultDto>.Fail(result.Message));
        }).WithTags("EOD");

        group.MapGet("/status/{date}", async (DateTime date, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetEodStatusQuery(date), ct);
            return Results.Ok(ApiResponse<EodStatusDto>.Ok(result));
        }).WithTags("EOD");

        return group;
    }
}
