using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.Commands;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.Queries;

namespace PostTrade.API.Features.ReferenceMaster;

public static class CdslDpMasterEndpoints
{
    public static RouteGroupBuilder MapCdslDpMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCdslDpMastersQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<CdslDpMasterDto>>.Ok(result));
        }).WithTags("CdslDpMaster");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCdslDpMasterByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<CdslDpMasterDto>.Fail("CDSL DP not found"))
                : Results.Ok(ApiResponse<CdslDpMasterDto>.Ok(result));
        }).WithTags("CdslDpMaster");

        group.MapPost("/import", async (IFormFile file, ISender sender, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportCdslDpMastersCommand(stream), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
        }).WithTags("CdslDpMaster").DisableAntiforgery();

        return group;
    }
}
