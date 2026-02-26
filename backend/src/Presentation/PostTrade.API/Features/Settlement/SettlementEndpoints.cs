using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Settlement.Batches.Commands;
using PostTrade.Application.Features.Settlement.Batches.DTOs;
using PostTrade.Application.Features.Settlement.Batches.Queries;
using PostTrade.Application.Features.Settlement.Obligations.Commands;
using PostTrade.Application.Features.Settlement.Obligations.DTOs;
using PostTrade.Application.Features.Settlement.Obligations.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.Settlement;

public static class SettlementEndpoints
{
    public static IEndpointRouteBuilder MapSettlementEndpoints(this IEndpointRouteBuilder app)
    {
        // --- Batches ---
        var batches = app.MapGroup("/api/settlement/batches").RequireAuthorization();

        batches.MapGet("/", async (ISender sender, CancellationToken ct, SettlementStatus? status = null) =>
        {
            var result = await sender.Send(new GetSettlementBatchesQuery(status), ct);
            return Results.Ok(ApiResponse<IEnumerable<SettlementBatchDto>>.Ok(result));
        }).WithTags("Settlement");

        batches.MapPost("/", async (CreateSettlementBatchCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/settlement/batches/{result.BatchId}", ApiResponse<SettlementBatchDto>.Ok(result, "Batch created"));
        }).WithTags("Settlement");

        batches.MapPut("/{id:guid}/process", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ProcessSettlementBatchCommand(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<SettlementBatchDto>.Fail("Batch not found"))
                : Results.Ok(ApiResponse<SettlementBatchDto>.Ok(result, "Batch processed"));
        }).WithTags("Settlement");

        // --- Obligations ---
        var obligations = app.MapGroup("/api/settlement/obligations").RequireAuthorization();

        obligations.MapGet("/", async (ISender sender, CancellationToken ct, Guid? batchId = null, ObligationStatus? status = null) =>
        {
            var result = await sender.Send(new GetSettlementObligationsQuery(batchId, status), ct);
            return Results.Ok(ApiResponse<IEnumerable<SettlementObligationDto>>.Ok(result));
        }).WithTags("Settlement");

        obligations.MapPut("/{id:guid}/settle", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SettleObligationCommand(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<SettlementObligationDto>.Fail("Obligation not found"))
                : Results.Ok(ApiResponse<SettlementObligationDto>.Ok(result, "Obligation settled"));
        }).WithTags("Settlement");

        return app;
    }
}
