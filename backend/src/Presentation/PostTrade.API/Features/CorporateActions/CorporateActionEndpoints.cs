using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.CorporateActions.Commands;
using PostTrade.Application.Features.CorporateActions.DTOs;
using PostTrade.Application.Features.CorporateActions.Queries;
using PostTrade.Domain.Enums;

namespace PostTrade.API.Features.CorporateActions;

public static class CorporateActionEndpoints
{
    public static IEndpointRouteBuilder MapCorporateActionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/corporate-actions").RequireAuthorization();

        group.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            Guid? instrumentId = null,
            CorporateActionType? actionType = null,
            CorporateActionStatus? status = null) =>
        {
            var result = await sender.Send(new GetCorporateActionsQuery(instrumentId, actionType, status), ct);
            return Results.Ok(ApiResponse<IEnumerable<CorporateActionDto>>.Ok(result));
        }).WithTags("CorporateActions");

        group.MapPost("/", async (CreateCorporateActionCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created(
                $"/api/corporate-actions/{result.CorporateActionId}",
                ApiResponse<CorporateActionDto>.Ok(result, "Corporate action created"));
        }).WithTags("CorporateActions");

        group.MapPut("/{id:guid}/process", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ProcessCorporateActionCommand(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<CorporateActionDto>.Fail("Corporate action not found"))
                : Results.Ok(ApiResponse<CorporateActionDto>.Ok(result, "Corporate action processed"));
        }).WithTags("CorporateActions");

        return app;
    }
}
