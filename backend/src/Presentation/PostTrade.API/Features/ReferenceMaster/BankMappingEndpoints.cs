using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Features.ReferenceMaster.BankMappings.Commands;
using PostTrade.Application.Features.ReferenceMaster.BankMappings.DTOs;
using PostTrade.Application.Features.ReferenceMaster.BankMappings.Queries;

namespace PostTrade.API.Features.ReferenceMaster;

public static class BankMappingEndpoints
{
    public static RouteGroupBuilder MapBankMappingEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string? bankCode, ISender sender, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(bankCode))
                return Results.Ok(ApiResponse<IEnumerable<BankMappingDto>>.Ok([]));

            var result = await sender.Send(new GetBankMappingsByBankCodeQuery(bankCode), ct);
            return Results.Ok(ApiResponse<IEnumerable<BankMappingDto>>.Ok(result));
        }).WithTags("BankMappings");

        group.MapGet("/ifsc/{ifsc}", async (string ifsc, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBankMappingByIFSCQuery(ifsc), ct);
            return result is null
                ? Results.NotFound(ApiResponse<BankMappingDto>.Fail("IFSC mapping not found"))
                : Results.Ok(ApiResponse<BankMappingDto>.Ok(result));
        }).WithTags("BankMappings");

        group.MapPost("/import", async (IFormFile file, ISender sender, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await sender.Send(new ImportBankMappingsCommand(stream), ct);
            return Results.Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
        }).WithTags("BankMappings").DisableAntiforgery();

        return group;
    }
}
