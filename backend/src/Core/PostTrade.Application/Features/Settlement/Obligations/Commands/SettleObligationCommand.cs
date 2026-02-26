using MediatR;
using PostTrade.Application.Features.Settlement.Obligations.DTOs;
using PostTrade.Application.Features.Settlement.Obligations.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Obligations.Commands;

public record SettleObligationCommand(Guid ObligationId) : IRequest<SettlementObligationDto?>;

public class SettleObligationCommandHandler : IRequestHandler<SettleObligationCommand, SettlementObligationDto?>
{
    private readonly IRepository<SettlementObligation> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public SettleObligationCommandHandler(
        IRepository<SettlementObligation> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<SettlementObligationDto?> Handle(SettleObligationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            o => o.ObligationId == request.ObligationId && o.TenantId == tenantId, cancellationToken);
        var obligation = results.FirstOrDefault();
        if (obligation is null) return null;

        if (obligation.Status == ObligationStatus.Settled)
            throw new InvalidOperationException("Obligation is already settled.");

        obligation.Status = ObligationStatus.Settled;
        obligation.SettledAt = DateTime.UtcNow;

        await _repo.UpdateAsync(obligation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetSettlementObligationsQueryHandler.ToDto(obligation);
    }
}
