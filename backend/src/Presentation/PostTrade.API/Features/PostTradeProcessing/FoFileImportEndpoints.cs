using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.PostTradeProcessing;

public static class FoFileImportEndpoints
{
    public static RouteGroupBuilder MapFoFileImportEndpoints(this RouteGroupBuilder group)
    {
        // ── Contract Master (prerequisite for trade import) ───────────────────

        group.MapPost("/import/contract-master", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NFO") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportFoContractMasterCommand(stream, tradingDate, exchange, file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "FO Contract Master imported"));
        })
        .DisableAntiforgery()
        .WithTags("FO File Import");

        // ── Trade files ───────────────────────────────────────────────────────

        group.MapPost("/import/trade", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NFO") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportFoTradeFileCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "FO Trade file imported"));
        })
        .DisableAntiforgery()
        .WithTags("FO File Import");

        // ── BhavCopy ─────────────────────────────────────────────────────────

        group.MapPost("/import/bhavcopy", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NFO") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportFoBhavCopyCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "FO BhavCopy file imported"));
        })
        .DisableAntiforgery()
        .WithTags("FO File Import");

        // ── STT ───────────────────────────────────────────────────────────────

        group.MapPost("/import/stt", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NFO") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportFoSttCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "FO STT file imported"));
        })
        .DisableAntiforgery()
        .WithTags("FO File Import");

        // ── Stamp Duty ────────────────────────────────────────────────────────

        group.MapPost("/import/stamp-duty", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NFO") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportFoStampDutyCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "FO Stamp Duty file imported"));
        })
        .DisableAntiforgery()
        .WithTags("FO File Import");

        // ── Position ──────────────────────────────────────────────────────────

        group.MapPost("/import/position", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NFO") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportFoPositionCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "FO Position file imported"));
        })
        .DisableAntiforgery()
        .WithTags("FO File Import");

        // ── Contract Master query ─────────────────────────────────────────────

        group.MapGet("/contract-masters", async (ISender sender, CancellationToken ct,
            string? exchange = null,
            DateOnly? tradingDate = null,
            string? symbol = null,
            int page = 1,
            int pageSize = 50) =>
        {
            var result = await sender.Send(new GetFoContractMastersQuery(exchange, tradingDate, symbol, page, pageSize), ct);
            return Results.Ok(ApiResponse<IEnumerable<FoContractMasterDto>>.Ok(result));
        })
        .WithTags("FO File Import");

        // ── Batch query endpoints ─────────────────────────────────────────────

        group.MapGet("/import/batches", async (ISender sender, CancellationToken ct,
            FoFileType? fileType = null,
            string? exchange = null,
            DateOnly? tradingDate = null,
            FoImportStatus? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(
                new GetFoImportBatchesQuery(fileType, exchange, tradingDate, status, page, pageSize), ct);
            return Results.Ok(ApiResponse<IEnumerable<FoImportBatchDto>>.Ok(result));
        })
        .WithTags("FO File Import");

        group.MapGet("/import/batches/{id:guid}/logs", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFoImportBatchLogsQuery(id), ct);
            return Results.Ok(ApiResponse<IEnumerable<FoImportBatchLogDto>>.Ok(result));
        })
        .WithTags("FO File Import");

        // ── Delete batch (allows reimport) ────────────────────────────────────

        group.MapDelete("/import/batches/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var deleted = await sender.Send(new DeleteFoImportBatchCommand(id), ct);
            return deleted
                ? Results.Ok(ApiResponse<string>.Ok("Batch deleted. You may reimport the file."))
                : Results.NotFound(ApiResponse<string>.Fail("Batch not found"));
        })
        .WithTags("FO File Import");

        return group;
    }
}
