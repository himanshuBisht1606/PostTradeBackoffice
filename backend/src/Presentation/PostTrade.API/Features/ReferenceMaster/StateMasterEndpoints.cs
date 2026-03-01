using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.ReferenceMaster.States.Commands;
using PostTrade.Application.Features.ReferenceMaster.States.DTOs;
using PostTrade.Application.Features.ReferenceMaster.States.Queries;

namespace PostTrade.API.Features.ReferenceMaster;

public static class StateMasterEndpoints
{
    public static RouteGroupBuilder MapStateMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStatesQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<StateMasterDto>>.Ok(result));
        }).WithTags("States");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStateByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<StateMasterDto>.Fail("State not found"))
                : Results.Ok(ApiResponse<StateMasterDto>.Ok(result));
        }).WithTags("States");

        group.MapPost("/", async (CreateStateCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/reference/states/{result.StateId}", ApiResponse<StateMasterDto>.Ok(result, "State created"));
        }).WithTags("States");

        group.MapPost("/import", async (IFormFile file, ISender sender, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportStatesCommand(stream), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
        }).WithTags("States").DisableAntiforgery();

        return group;
    }
}
