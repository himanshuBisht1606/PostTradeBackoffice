using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Instruments.Commands;
using PostTrade.Application.Features.MasterSetup.Instruments.DTOs;
using PostTrade.Application.Features.MasterSetup.Instruments.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.MasterSetup;

public static class InstrumentEndpoints
{
    public static RouteGroupBuilder MapInstrumentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct, Guid? exchangeId = null, InstrumentType? type = null) =>
        {
            var result = await sender.Send(new GetInstrumentsQuery(exchangeId, type), ct);
            return Results.Ok(ApiResponse<IEnumerable<InstrumentDto>>.Ok(result));
        }).WithTags("Instruments");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetInstrumentByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<InstrumentDto>.Fail("Instrument not found"))
                : Results.Ok(ApiResponse<InstrumentDto>.Ok(result));
        }).WithTags("Instruments");

        group.MapPost("/", async (CreateInstrumentCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/instruments/{result.InstrumentId}", ApiResponse<InstrumentDto>.Ok(result, "Instrument created"));
        }).WithTags("Instruments");

        group.MapPut("/{id:guid}", async (Guid id, UpdateInstrumentCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { InstrumentId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<InstrumentDto>.Fail("Instrument not found"))
                : Results.Ok(ApiResponse<InstrumentDto>.Ok(result, "Instrument updated"));
        }).WithTags("Instruments");

        return group;
    }
}
