using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.Commands;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.DTOs;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class ExchangeSegmentEndpoints
{
    public static RouteGroupBuilder MapExchangeSegmentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct, Guid? exchangeId = null, Guid? segmentId = null) =>
        {
            var result = await sender.Send(new GetExchangeSegmentsQuery(exchangeId, segmentId), ct);
            return Results.Ok(ApiResponse<IEnumerable<ExchangeSegmentDto>>.Ok(result));
        }).WithTags("ExchangeSegments");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExchangeSegmentByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<ExchangeSegmentDto>.Fail("ExchangeSegment not found"))
                : Results.Ok(ApiResponse<ExchangeSegmentDto>.Ok(result));
        }).WithTags("ExchangeSegments");

        group.MapPost("/", async (CreateExchangeSegmentCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/exchange-segments/{result.ExchangeSegmentId}",
                ApiResponse<ExchangeSegmentDto>.Ok(result, "ExchangeSegment created"));
        }).WithTags("ExchangeSegments");

        group.MapPut("/{id:guid}", async (Guid id, UpdateExchangeSegmentCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { ExchangeSegmentId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<ExchangeSegmentDto>.Fail("ExchangeSegment not found"))
                : Results.Ok(ApiResponse<ExchangeSegmentDto>.Ok(result, "ExchangeSegment updated"));
        }).WithTags("ExchangeSegments");

        return group;
    }
}
