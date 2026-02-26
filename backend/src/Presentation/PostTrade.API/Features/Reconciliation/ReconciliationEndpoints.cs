using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.Reconciliation.Commands;
using PostTrade.Application.Features.Reconciliation.DTOs;
using PostTrade.Application.Features.Reconciliation.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.Reconciliation;

public static class ReconciliationEndpoints
{
    public static IEndpointRouteBuilder MapReconciliationEndpoints(this IEndpointRouteBuilder app)
    {
        // --- Reconciliation records ---
        var recon = app.MapGroup("/api/reconciliation").RequireAuthorization();

        recon.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            DateTime? reconDate = null,
            ReconType? reconType = null,
            ReconStatus? status = null) =>
        {
            var result = await sender.Send(new GetReconciliationsQuery(reconDate, reconType, status), ct);
            return Results.Ok(ApiResponse<IEnumerable<ReconciliationDto>>.Ok(result));
        }).WithTags("Reconciliation");

        recon.MapPost("/run", async (RunReconciliationCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created(
                $"/api/reconciliation/{result.ReconId}",
                ApiResponse<ReconciliationDto>.Ok(result, "Reconciliation run completed"));
        }).WithTags("Reconciliation");

        // --- Recon Exceptions ---
        var exceptions = app.MapGroup("/api/reconciliation/exceptions").RequireAuthorization();

        exceptions.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            Guid? reconId = null,
            ExceptionStatus? status = null) =>
        {
            var result = await sender.Send(new GetReconExceptionsQuery(reconId, status), ct);
            return Results.Ok(ApiResponse<IEnumerable<ReconExceptionDto>>.Ok(result));
        }).WithTags("Reconciliation");

        exceptions.MapPut("/{id:guid}/resolve", async (
            Guid id,
            ResolveReconExceptionCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command with { ExceptionId = id }, ct);
            return result is null
                ? Results.NotFound(ApiResponse<ReconExceptionDto>.Fail("Exception not found"))
                : Results.Ok(ApiResponse<ReconExceptionDto>.Ok(result, "Exception resolved"));
        }).WithTags("Reconciliation");

        return app;
    }
}
