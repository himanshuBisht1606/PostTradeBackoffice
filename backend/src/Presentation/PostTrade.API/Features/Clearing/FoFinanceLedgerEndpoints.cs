using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

namespace PostTrade.API.Features.Clearing;

public static class FoFinanceLedgerEndpoints
{
    public static RouteGroupBuilder MapFoFinanceLedgerEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/clearing/fo/finance-ledger/compute
        // Body: { tradeDate, exchange }
        group.MapPost("/compute", async (PostFoFinanceLedgerCommand command, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var result = await sender.Send(command, ct);
                return Results.Ok(ApiResponse<PostFoFinanceLedgerResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ApiResponse<string>.Fail(ex.Message));
            }
        }).WithTags("FO Finance Ledger");

        // GET /api/clearing/fo/finance-ledger?tradeDate=&exchange=&clientCode=
        group.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            DateOnly? tradeDate = null,
            string? exchange = null,
            string? clientCode = null) =>
        {
            var result = await sender.Send(new GetFoFinanceLedgerQuery(tradeDate, exchange, clientCode), ct);
            return Results.Ok(ApiResponse<IEnumerable<FoFinanceLedgerDto>>.Ok(result));
        }).WithTags("FO Finance Ledger");

        // DELETE /api/clearing/fo/finance-ledger?tradeDate=&exchange=
        group.MapDelete("/", async (
            ISender sender,
            CancellationToken ct,
            DateOnly tradeDate,
            string exchange) =>
        {
            var deleted = await sender.Send(new DeleteFoFinanceLedgerCommand(tradeDate, exchange), ct);
            return Results.Ok(ApiResponse<string>.Ok(
                $"Deleted {deleted} finance ledger row(s) for {tradeDate:yyyy-MM-dd} / {exchange}. You may recompute."));
        }).WithTags("FO Finance Ledger");

        return group;
    }
}
