using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Trading.PnL.DTOs;
using PostTrade.Application.Features.Trading.Trades.Commands;
using PostTrade.Application.Features.Trading.Trades.DTOs;
using PostTrade.Application.Features.Trading.Trades.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.Trading;

public static class TradeEndpoints
{
    public static RouteGroupBuilder MapTradeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            ISender sender, CancellationToken ct,
            DateTime? fromDate = null, DateTime? toDate = null,
            Guid? clientId = null, Guid? instrumentId = null,
            TradeStatus? status = null) =>
        {
            var result = await sender.Send(new GetTradesQuery(fromDate, toDate, clientId, instrumentId, status), ct);
            return Results.Ok(ApiResponse<IEnumerable<TradeDto>>.Ok(result));
        }).WithTags("Trades");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTradeByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<TradeDto>.Fail("Trade not found"))
                : Results.Ok(ApiResponse<TradeDto>.Ok(result));
        }).WithTags("Trades");

        group.MapPost("/", async (BookTradeCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/trades/{result.TradeId}", ApiResponse<TradeDto>.Ok(result, "Trade booked"));
        }).WithTags("Trades");

        group.MapPut("/{id:guid}/cancel", async (Guid id, CancelTradeCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { TradeId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<TradeDto>.Fail("Trade not found"))
                : Results.Ok(ApiResponse<TradeDto>.Ok(result, "Trade cancelled"));
        }).WithTags("Trades");

        group.MapGet("/{id:guid}/pnl", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTradePnLQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<PnLSnapshotDto>.Fail("P&L not found for this trade"))
                : Results.Ok(ApiResponse<PnLSnapshotDto>.Ok(result));
        }).WithTags("Trades");

        return group;
    }
}
