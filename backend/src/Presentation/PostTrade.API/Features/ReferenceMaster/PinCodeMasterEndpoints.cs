using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.ReferenceMaster.PinCodes.Commands;
using PostTrade.Application.Features.ReferenceMaster.PinCodes.DTOs;
using PostTrade.Application.Features.ReferenceMaster.PinCodes.Queries;

namespace PostTrade.API.Features.ReferenceMaster;

public static class PinCodeMasterEndpoints
{
    public static RouteGroupBuilder MapPinCodeMasterEndpoints(this RouteGroupBuilder group)
    {
        // GET /api/reference/pin-codes?stateCode=MH  (optional filter)
        group.MapGet("/", async (string? stateCode, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPinCodeMastersQuery(stateCode), ct);
            return Results.Ok(ApiResponse<IEnumerable<PinCodeMasterDto>>.Ok(result));
        }).WithTags("PinCodes");

        // GET /api/reference/pin-codes/{code}  — lookup by pincode string (used in onboarding)
        group.MapGet("/{code}", async (string code, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPinCodeByCodeQuery(code), ct);
            return result is null
                ? Results.NotFound(ApiResponse<PinCodeMasterDto>.Fail("Pin code not found"))
                : Results.Ok(ApiResponse<PinCodeMasterDto>.Ok(result));
        }).WithTags("PinCodes");

        group.MapPost("/import", async (IFormFile file, ISender sender, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportPinCodeMastersCommand(stream), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
        }).WithTags("PinCodes").DisableAntiforgery();

        return group;
    }
}
