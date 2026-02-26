using MediatR;
using PostTrade.Application.Features.CorporateActions.DTOs;
using PostTrade.Application.Features.CorporateActions.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.CorporateActions.Commands;

public record ProcessCorporateActionCommand(Guid CorporateActionId) : IRequest<CorporateActionDto?>;

public class ProcessCorporateActionCommandHandler : IRequestHandler<ProcessCorporateActionCommand, CorporateActionDto?>
{
    private readonly IRepository<CorporateAction> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ProcessCorporateActionCommandHandler(
        IRepository<CorporateAction> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<CorporateActionDto?> Handle(ProcessCorporateActionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var results = await _repo.FindAsync(
            a => a.CorporateActionId == request.CorporateActionId && a.TenantId == tenantId,
            cancellationToken);

        var action = results.FirstOrDefault();
        if (action is null) return null;

        if (action.Status != CorporateActionStatus.Announced)
            throw new InvalidOperationException($"Corporate action cannot be processed in '{action.Status}' status.");

        action.Status = CorporateActionStatus.Processing;
        await _repo.UpdateAsync(action, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Processing logic (adjustments to positions/ledger) would be triggered here
        // via domain events or additional services in a full implementation.

        action.Status = CorporateActionStatus.Completed;
        action.IsProcessed = true;
        action.ProcessedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(action, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetCorporateActionsQueryHandler.ToDto(action);
    }
}
