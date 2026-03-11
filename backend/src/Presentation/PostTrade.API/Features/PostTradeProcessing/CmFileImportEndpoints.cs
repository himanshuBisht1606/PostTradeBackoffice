using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.PostTradeProcessing;

public static class CmFileImportEndpoints
{
    public static RouteGroupBuilder MapCmFileImportEndpoints(this RouteGroupBuilder group)
    {
        // ── Import endpoints ──────────────────────────────────────────────────

        group.MapPost("/import/trade", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmTradeFileCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "Trade file imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        group.MapPost("/import/bhavcopy", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmBhavCopyCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "BhavCopy file imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        group.MapPost("/import/margin", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmMarginCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "Margin file imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        group.MapPost("/import/obligation", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmObligationCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "Obligation file imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        group.MapPost("/import/stt", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmSttCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "STT file imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        group.MapPost("/import/stamp-duty", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmStampDutyCommand(stream, tradingDate, exchange, "ManualUpload", file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "Stamp Duty file imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        // ── Master import endpoints ───────────────────────────────────────────

        group.MapPost("/import/settlement-master", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmSettlementMasterCommand(stream, tradingDate, exchange, file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "Settlement Master imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        group.MapPost("/import/scrip-master", async (IFormFile file, ISender sender, CancellationToken ct,
            DateOnly tradingDate, string exchange = "NSE") =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new ImportCmScripMasterCommand(stream, tradingDate, exchange, file.FileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, "Scrip Master imported"));
        })
        .DisableAntiforgery()
        .WithTags("CM File Import");

        // ── Exchange download endpoints (no file upload needed) ───────────────

        group.MapPost("/import/scrip-master/download/nse", async (
            ISender sender,
            IExchangeScripDownloader downloader,
            CancellationToken ct,
            DateOnly tradingDate) =>
        {
            await using var stream = await downloader.DownloadNseAsync(tradingDate, ct);
            var fileName = $"NSE_CM_security_{tradingDate:ddMMyyyy}.csv";
            var result = await sender.Send(
                new ImportCmScripMasterCommand(stream, tradingDate, "NSE", fileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, $"NSE Scrip Master downloaded and imported for {tradingDate:dd-MM-yyyy}"));
        })
        .WithTags("CM File Import");

        group.MapPost("/import/scrip-master/download/bse", async (
            ISender sender,
            IExchangeScripDownloader downloader,
            CancellationToken ct,
            DateOnly tradingDate) =>
        {
            await using var stream = await downloader.DownloadBseAsync(ct);
            var fileName = $"SCRIP_BSE_{tradingDate:ddMMyyyy}.TXT";
            var result = await sender.Send(
                new ImportCmScripMasterCommand(stream, tradingDate, "BSE", fileName), ct);
            return Results.Ok(ApiResponse<object>.Ok(result, $"BSE Scrip Master downloaded and imported for {tradingDate:dd-MM-yyyy}"));
        })
        .WithTags("CM File Import");

        // ── Master query endpoints ────────────────────────────────────────────

        group.MapGet("/settlement-masters", async (ISender sender, CancellationToken ct,
            string? exchange = null,
            DateOnly? tradingDate = null) =>
        {
            var result = await sender.Send(new GetCmSettlementMastersQuery(exchange, tradingDate), ct);
            return Results.Ok(ApiResponse<IEnumerable<CmSettlementMasterDto>>.Ok(result));
        })
        .WithTags("CM File Import");

        group.MapGet("/scrip-masters", async (ISender sender, CancellationToken ct,
            string? exchange = null,
            DateOnly? tradingDate = null,
            string? symbol = null,
            string? isin = null,
            int page = 1,
            int pageSize = 50) =>
        {
            var result = await sender.Send(new GetCmScripMastersQuery(exchange, tradingDate, symbol, isin, page, pageSize), ct);
            return Results.Ok(ApiResponse<IEnumerable<CmScripMasterDto>>.Ok(result));
        })
        .WithTags("CM File Import");

        // ── Batch query endpoints ─────────────────────────────────────────────

        group.MapGet("/import/batches", async (ISender sender, CancellationToken ct,
            CmFileType? fileType = null,
            string? exchange = null,
            DateOnly? tradingDate = null,
            CmImportStatus? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(
                new GetCmImportBatchesQuery(fileType, exchange, tradingDate, status, page, pageSize), ct);
            return Results.Ok(ApiResponse<IEnumerable<CmImportBatchDto>>.Ok(result));
        })
        .WithTags("CM File Import");

        group.MapGet("/import/batches/{id:guid}/logs", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCmImportBatchLogsQuery(id), ct);
            return Results.Ok(ApiResponse<IEnumerable<CmImportBatchLogDto>>.Ok(result));
        })
        .WithTags("CM File Import");

        // ── Delete batch (allows reimport) ────────────────────────────────────

        group.MapDelete("/import/batches/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var deleted = await sender.Send(new DeleteCmImportBatchCommand(id), ct);
            return deleted
                ? Results.Ok(ApiResponse<string>.Ok("Batch deleted. You may reimport the file."))
                : Results.NotFound(ApiResponse<string>.Fail("Batch not found"));
        })
        .WithTags("CM File Import");

        return group;
    }
}
