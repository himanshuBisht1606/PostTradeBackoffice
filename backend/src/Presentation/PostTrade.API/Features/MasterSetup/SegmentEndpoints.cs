using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Segments.Commands;
using PostTrade.Application.Features.MasterSetup.Segments.DTOs;
using PostTrade.Application.Features.MasterSetup.Segments.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class SegmentEndpoints
{
    public static RouteGroupBuilder MapSegmentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSegmentsQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<SegmentDto>>.Ok(result));
        }).WithTags("Segments");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSegmentByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<SegmentDto>.Fail("Segment not found"))
                : Results.Ok(ApiResponse<SegmentDto>.Ok(result));
        }).WithTags("Segments");

        group.MapPost("/", async (CreateSegmentCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/segments/{result.SegmentId}", ApiResponse<SegmentDto>.Ok(result, "Segment created"));
        }).WithTags("Segments");

        group.MapPut("/{id:guid}", async (Guid id, UpdateSegmentCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { SegmentId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<SegmentDto>.Fail("Segment not found"))
                : Results.Ok(ApiResponse<SegmentDto>.Ok(result, "Segment updated"));
        }).WithTags("Segments");

        return group;
    }
}
