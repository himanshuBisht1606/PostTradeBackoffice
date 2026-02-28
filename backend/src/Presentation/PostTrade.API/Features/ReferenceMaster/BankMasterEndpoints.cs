using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.ReferenceMaster.Banks.Commands;
using PostTrade.Application.Features.ReferenceMaster.Banks.DTOs;
using PostTrade.Application.Features.ReferenceMaster.Banks.Queries;

namespace PostTrade.API.Features.ReferenceMaster;

public static class BankMasterEndpoints
{
    public static RouteGroupBuilder MapBankMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBankMastersQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<BankMasterDto>>.Ok(result));
        }).WithTags("Banks");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBankMasterByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<BankMasterDto>.Fail("Bank not found"))
                : Results.Ok(ApiResponse<BankMasterDto>.Ok(result));
        }).WithTags("Banks");

        group.MapPost("/import", async (IFormFile file, ISender sender, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportBankMastersCommand(stream), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
        }).WithTags("Banks").DisableAntiforgery();

        return group;
    }
}
