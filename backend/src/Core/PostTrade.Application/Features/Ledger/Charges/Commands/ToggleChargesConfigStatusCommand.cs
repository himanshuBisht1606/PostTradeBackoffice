using MediatR;
using PostTrade.Application.Features.Ledger.Charges.DTOs;
using PostTrade.Application.Features.Ledger.Charges.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;

namespace PostTrade.Application.Features.Ledger.Charges.Commands;

public record ToggleChargesConfigStatusCommand(Guid ChargesConfigId) : IRequest<ChargesConfigDto>;

public class ToggleChargesConfigStatusCommandHandler : IRequestHandler<ToggleChargesConfigStatusCommand, ChargesConfigDto>
{
    private readonly IRepository<ChargesConfiguration> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ToggleChargesConfigStatusCommandHandler(
        IRepository<ChargesConfiguration> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ChargesConfigDto> Handle(ToggleChargesConfigStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var config = await _repo.FirstOrDefaultAsync(
            c => c.ChargesConfigId == request.ChargesConfigId && c.TenantId == tenantId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Charges config {request.ChargesConfigId} not found.");

        config.IsActive = !config.IsActive;
        await _repo.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetChargesConfigQueryHandler.ToDto(config);
    }
}
