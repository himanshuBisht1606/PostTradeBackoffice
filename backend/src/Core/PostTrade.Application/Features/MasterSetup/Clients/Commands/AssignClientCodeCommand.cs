using FluentValidation;
using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

public record AssignClientCodeCommand(Guid ClientId, string ClientCode) : IRequest;

public class AssignClientCodeCommandValidator : AbstractValidator<AssignClientCodeCommand>
{
    public AssignClientCodeCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientCode).NotEmpty().MaximumLength(20);
    }
}

public class AssignClientCodeCommandHandler : IRequestHandler<AssignClientCodeCommand>
{
    private readonly IRepository<Client> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignClientCodeCommandHandler(
        IRepository<Client> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task Handle(AssignClientCodeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var clients = await _repo.FindAsync(
            c => c.ClientId == request.ClientId && c.TenantId == tenantId,
            cancellationToken);
        var client = clients.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Client {request.ClientId} not found.");

        // Check uniqueness of ClientCode within tenant (excluding self)
        var existing = await _repo.FindAsync(
            c => c.TenantId == tenantId
              && c.ClientCode == request.ClientCode
              && c.ClientId != request.ClientId,
            cancellationToken);
        if (existing.Any())
            throw new InvalidOperationException($"Client code '{request.ClientCode}' is already assigned to another client.");

        client.ClientCode = request.ClientCode;
        client.Status = ClientStatus.Active;

        await _repo.UpdateAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
