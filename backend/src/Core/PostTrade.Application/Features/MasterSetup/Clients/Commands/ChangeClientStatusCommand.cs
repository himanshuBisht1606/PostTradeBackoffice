using FluentValidation;
using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

public record ChangeClientStatusCommand(Guid ClientId, ClientStatus Status) : IRequest<bool>;

public class ChangeClientStatusCommandValidator : AbstractValidator<ChangeClientStatusCommand>
{
    public ChangeClientStatusCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class ChangeClientStatusCommandHandler : IRequestHandler<ChangeClientStatusCommand, bool>
{
    private readonly IRepository<Client> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ChangeClientStatusCommandHandler(
        IRepository<Client> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(ChangeClientStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            c => c.ClientId == request.ClientId && c.TenantId == tenantId,
            cancellationToken);
        var client = results.FirstOrDefault();
        if (client is null) return false;

        client.Status = request.Status;
        await _repo.UpdateAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
