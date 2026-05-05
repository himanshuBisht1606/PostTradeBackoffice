using MediatR;
using PostTrade.Application.Features.MasterSetup.ClientSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.ClientSegments.Commands;

public record DeactivateClientSegmentCommand(Guid ActivationId) : IRequest<ClientSegmentActivationDto?>;

public class DeactivateClientSegmentCommandHandler : IRequestHandler<DeactivateClientSegmentCommand, ClientSegmentActivationDto?>
{
    private readonly IRepository<ClientSegmentActivation> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeactivateClientSegmentCommandHandler(IRepository<ClientSegmentActivation> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ClientSegmentActivationDto?> Handle(DeactivateClientSegmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(a => a.ActivationId == request.ActivationId && a.TenantId == tenantId, cancellationToken);
        var activation = results.FirstOrDefault();
        if (activation is null) return null;

        activation.Status = ActivationStatus.Inactive;
        activation.DeactivatedOn = DateTime.UtcNow;

        await _repo.UpdateAsync(activation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientSegmentActivationDto(activation.ActivationId, activation.TenantId,
            activation.ClientId, activation.ExchangeSegmentId, activation.Status,
            activation.ExposureLimit, activation.MarginType, activation.ActivatedOn, activation.DeactivatedOn);
    }
}
