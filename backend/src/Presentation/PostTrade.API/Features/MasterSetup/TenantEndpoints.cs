using MediatR;
using PostTrade.API.Common;
using PostTrade.Application.Features.MasterSetup.Tenants.Commands;
using PostTrade.Application.Features.MasterSetup.Tenants.DTOs;
using PostTrade.Application.Features.MasterSetup.Tenants.Queries;

namespace PostTrade.API.Features.MasterSetup;

public static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenantEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTenantsQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<TenantDto>>.Ok(result));
        }).WithTags("Tenants");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTenantByIdQuery(id), ct);
            return result is null
                ? Results.NotFound(ApiResponse<TenantDto>.Fail("Tenant not found"))
                : Results.Ok(ApiResponse<TenantDto>.Ok(result));
        }).WithTags("Tenants");

        group.MapPost("/", async (CreateTenantCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/tenants/{result.TenantId}", ApiResponse<TenantDto>.Ok(result, "Tenant created"));
        }).WithTags("Tenants");

        group.MapPut("/{id:guid}", async (Guid id, UpdateTenantCommand command, ISender sender, CancellationToken ct) =>
        {
            var cmd = command with { TenantId = id };
            var result = await sender.Send(cmd, ct);
            return result is null
                ? Results.NotFound(ApiResponse<TenantDto>.Fail("Tenant not found"))
                : Results.Ok(ApiResponse<TenantDto>.Ok(result, "Tenant updated"));
        }).WithTags("Tenants");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteTenantCommand(id), ct);
            return result
                ? Results.Ok(ApiResponse<bool>.Ok(true, "Tenant deleted"))
                : Results.NotFound(ApiResponse<bool>.Fail("Tenant not found"));
        }).WithTags("Tenants");

        return group;
    }
}
