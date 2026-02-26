using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Exchanges.Commands;
using PostTrade.Application.Features.MasterSetup.Exchanges.DTOs;
using PostTrade.Application.Features.MasterSetup.Exchanges.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class ExchangeEndpoints
{
    public static RouteGroupBuilder MapExchangeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExchangesQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<ExchangeDto>>.Ok(result));
        }).WithTags("Exchanges");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExchangeByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<ExchangeDto>.Fail("Exchange not found"))
                : Results.Ok(ApiResponse<ExchangeDto>.Ok(result));
        }).WithTags("Exchanges");

        group.MapPost("/", async (CreateExchangeCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/exchanges/{result.ExchangeId}", ApiResponse<ExchangeDto>.Ok(result, "Exchange created"));
        }).WithTags("Exchanges");

        group.MapPut("/{id:guid}", async (Guid id, UpdateExchangeCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { ExchangeId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<ExchangeDto>.Fail("Exchange not found"))
                : Results.Ok(ApiResponse<ExchangeDto>.Ok(result, "Exchange updated"));
        }).WithTags("Exchanges");

        return group;
    }
}
