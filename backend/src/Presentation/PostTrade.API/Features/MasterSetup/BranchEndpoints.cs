using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Branches.Commands;
using PostTrade.Application.Features.MasterSetup.Branches.DTOs;
using PostTrade.Application.Features.MasterSetup.Branches.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class BranchEndpoints
{
    public static RouteGroupBuilder MapBranchEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBranchesQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<BranchDto>>.Ok(result));
        }).WithTags("Branches");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBranchByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<BranchDto>.Fail("Branch not found"))
                : Results.Ok(ApiResponse<BranchDto>.Ok(result));
        }).WithTags("Branches");

        group.MapPost("/", async (CreateBranchCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/branches/{result.BranchId}", ApiResponse<BranchDto>.Ok(result, "Branch created"));
        }).WithTags("Branches");

        group.MapPut("/{id:guid}", async (Guid id, UpdateBranchCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { BranchId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<BranchDto>.Fail("Branch not found"))
                : Results.Ok(ApiResponse<BranchDto>.Ok(result, "Branch updated"));
        }).WithTags("Branches");

        return group;
    }
}
