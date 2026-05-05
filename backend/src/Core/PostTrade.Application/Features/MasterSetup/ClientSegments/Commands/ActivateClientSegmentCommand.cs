using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.ClientSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.ClientSegments.Commands;

public record ActivateClientSegmentCommand(
    Guid ClientId,
    Guid ExchangeSegmentId,
    decimal? ExposureLimit,
    MarginType MarginType
) : IRequest<ClientSegmentActivationDto>;

public class ActivateClientSegmentCommandValidator : AbstractValidator<ActivateClientSegmentCommand>
{
    public ActivateClientSegmentCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ExchangeSegmentId).NotEmpty();
        RuleFor(x => x.ExposureLimit).GreaterThan(0).When(x => x.ExposureLimit.HasValue);
    }
}

public class ActivateClientSegmentCommandHandler : IRequestHandler<ActivateClientSegmentCommand, ClientSegmentActivationDto>
{
    private readonly IRepository<ClientSegmentActivation> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ActivateClientSegmentCommandHandler(IRepository<ClientSegmentActivation> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ClientSegmentActivationDto> Handle(ActivateClientSegmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var activation = new ClientSegmentActivation
        {
            ActivationId = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = request.ClientId,
            ExchangeSegmentId = request.ExchangeSegmentId,
            Status = ActivationStatus.Active,
            ExposureLimit = request.ExposureLimit,
            MarginType = request.MarginType,
            ActivatedOn = DateTime.UtcNow
        };

        await _repo.AddAsync(activation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientSegmentActivationDto(activation.ActivationId, activation.TenantId,
            activation.ClientId, activation.ExchangeSegmentId, activation.Status,
            activation.ExposureLimit, activation.MarginType, activation.ActivatedOn, activation.DeactivatedOn);
    }
}
