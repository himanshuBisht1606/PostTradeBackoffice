using MediatR;
using Microsoft.AspNetCore.Mvc;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.PostTrade.FO.Commands;

namespace PostTrade.API.Features.PostTrade;

public static class FoImportEndpoints
{
    public static RouteGroupBuilder MapFoImportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/import/contract-master", async (
            [FromQuery] string tradingDate,
            [FromQuery] string exchange,
            IFormFile file,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!DateOnly.TryParse(tradingDate, out var date))
                return Results.BadRequest(ApiResponse<ImportResultDto>.Fail($"Invalid tradingDate: '{tradingDate}'. Expected format: yyyy-MM-dd"));

            if (string.IsNullOrWhiteSpace(exchange))
                return Results.BadRequest(ApiResponse<ImportResultDto>.Fail("exchange query parameter is required"));

            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportFoContractMasterCommand(stream, exchange, date), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "FO contract master import completed"));
        }).WithTags("FO Import").DisableAntiforgery();

        return group;
    }
}
