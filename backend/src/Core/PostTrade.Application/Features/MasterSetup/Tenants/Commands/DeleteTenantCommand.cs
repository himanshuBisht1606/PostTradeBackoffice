using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Tenants.Commands;

public record DeleteTenantCommand(Guid TenantId) : IRequest<bool>;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, bool>
{
    private readonly IRepository<Tenant> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTenantCommandHandler(IRepository<Tenant> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var results = await _repo.FindAsync(t => t.TenantId == request.TenantId, cancellationToken);
        var tenant = results.FirstOrDefault();
        if (tenant is null) return false;

        tenant.Status = TenantStatus.Inactive;
        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
