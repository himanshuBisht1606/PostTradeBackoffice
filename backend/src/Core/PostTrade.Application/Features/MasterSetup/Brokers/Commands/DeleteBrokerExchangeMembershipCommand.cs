using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

public record DeleteBrokerExchangeMembershipCommand(Guid BrokerId, Guid BrokerExchangeMembershipId) : IRequest<bool>;

public class DeleteBrokerExchangeMembershipCommandHandler
    : IRequestHandler<DeleteBrokerExchangeMembershipCommand, bool>
{
    private readonly IRepository<BrokerExchangeMembership> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteBrokerExchangeMembershipCommandHandler(
        IRepository<BrokerExchangeMembership> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(DeleteBrokerExchangeMembershipCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            m => m.BrokerExchangeMembershipId == request.BrokerExchangeMembershipId
              && m.BrokerId == request.BrokerId
              && m.TenantId == tenantId,
            cancellationToken);

        var membership = results.FirstOrDefault();
        if (membership is null) return false;

        await _repo.DeleteAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
