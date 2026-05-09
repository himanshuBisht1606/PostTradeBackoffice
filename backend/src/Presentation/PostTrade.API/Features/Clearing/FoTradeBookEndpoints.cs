using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

namespace PostTrade.API.Features.Clearing;

public static class FoTradeBookEndpoints
{
    public static RouteGroupBuilder MapFoTradeBookEndpoints(this RouteGroupBuilder group)
    {
        // GET /api/clearing/fo/trade-book?dateFrom=&dateTo=&exchange=&symbol=&clientCode=&optionType=&side=&contractType=&page=&pageSize=
        group.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            DateOnly dateFrom,
            DateOnly dateTo,
            string? exchange = null,
            string? symbol = null,
            string? clientCode = null,
            string? optionType = null,
            string? side = null,
            string? contractType = null,
            int page = 1,
            int pageSize = 50) =>
        {
            var result = await sender.Send(new GetFoTradeBookQuery(
                dateFrom, dateTo, exchange, symbol, clientCode, optionType, side, contractType, page, pageSize), ct);
            return Results.Ok(ApiResponse<FoTradeBookPagedDto>.Ok(result));
        }).WithTags("FO Trade Book");

        return group;
    }
}
