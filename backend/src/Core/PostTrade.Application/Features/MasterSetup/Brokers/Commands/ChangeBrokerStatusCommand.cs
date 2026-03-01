using FluentValidation;
using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

public record ChangeBrokerStatusCommand(Guid BrokerId, BrokerStatus Status) : IRequest<bool>;

public class ChangeBrokerStatusCommandValidator : AbstractValidator<ChangeBrokerStatusCommand>
{
    public ChangeBrokerStatusCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class ChangeBrokerStatusCommandHandler : IRequestHandler<ChangeBrokerStatusCommand, bool>
{
    private readonly IRepository<Broker> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ChangeBrokerStatusCommandHandler(
        IRepository<Broker> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(ChangeBrokerStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            b => b.BrokerId == request.BrokerId && b.TenantId == tenantId,
            cancellationToken);
        var broker = results.FirstOrDefault();
        if (broker is null) return false;

        broker.Status = request.Status;
        await _repo.UpdateAsync(broker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
