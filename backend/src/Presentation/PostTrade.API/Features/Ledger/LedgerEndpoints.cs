using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Ledger.Charges.Commands;
using PostTrade.Application.Features.Ledger.Charges.DTOs;
using PostTrade.Application.Features.Ledger.Charges.Queries;
using PostTrade.Application.Features.Ledger.Entries.Commands;
using PostTrade.Application.Features.Ledger.Entries.DTOs;
using PostTrade.Application.Features.Ledger.Entries.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.Ledger;

public static class LedgerEndpoints
{
    public static IEndpointRouteBuilder MapLedgerEndpoints(this IEndpointRouteBuilder app)
    {
        // --- Ledger Entries ---
        var entries = app.MapGroup("/api/ledger/entries").RequireAuthorization();

        entries.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            Guid? clientId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            LedgerType? ledgerType = null,
            EntryType? entryType = null) =>
        {
            var result = await sender.Send(
                new GetLedgerEntriesQuery(clientId, fromDate, toDate, ledgerType, entryType), ct);
            return Results.Ok(ApiResponse<IEnumerable<LedgerEntryDto>>.Ok(result));
        }).WithTags("Ledger");

        entries.MapPost("/", async (PostLedgerEntryCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created(
                $"/api/ledger/entries/{result.LedgerId}",
                ApiResponse<LedgerEntryDto>.Ok(result, "Ledger entry posted"));
        }).WithTags("Ledger");

        // --- Charges Configuration ---
        var charges = app.MapGroup("/api/ledger/charges").RequireAuthorization();

        charges.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            ChargeType? chargeType = null,
            bool? isActive = null) =>
        {
            var result = await sender.Send(new GetChargesConfigQuery(chargeType, isActive), ct);
            return Results.Ok(ApiResponse<IEnumerable<ChargesConfigDto>>.Ok(result));
        }).WithTags("Ledger");

        charges.MapPost("/", async (CreateChargesConfigCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created(
                $"/api/ledger/charges/{result.ChargesConfigId}",
                ApiResponse<ChargesConfigDto>.Ok(result, "Charges configuration created"));
        }).WithTags("Ledger");

        return app;
    }
}
