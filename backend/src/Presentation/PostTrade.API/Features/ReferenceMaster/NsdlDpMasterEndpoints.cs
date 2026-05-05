using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.Commands;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.Queries;

namespace PostTrade.API.Features.ReferenceMaster;

public static class NsdlDpMasterEndpoints
{
    public static RouteGroupBuilder MapNsdlDpMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetNsdlDpMastersQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<NsdlDpMasterDto>>.Ok(result));
        }).WithTags("NsdlDpMaster");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetNsdlDpMasterByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<NsdlDpMasterDto>.Fail("NSDL DP not found"))
                : Results.Ok(ApiResponse<NsdlDpMasterDto>.Ok(result));
        }).WithTags("NsdlDpMaster");

        group.MapPost("/import", async (IFormFile file, ISender sender, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportNsdlDpMastersCommand(stream), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
        }).WithTags("NsdlDpMaster").DisableAntiforgery();

        return group;
    }
}
