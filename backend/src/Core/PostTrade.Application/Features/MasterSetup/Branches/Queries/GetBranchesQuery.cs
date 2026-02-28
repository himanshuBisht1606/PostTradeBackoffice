using MediatR;
using PostTrade.Application.Features.MasterSetup.Branches.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Branches.Queries;

public record GetBranchesQuery : IRequest<IEnumerable<BranchDto>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, IEnumerable<BranchDto>>
{
    private readonly IRepository<Branch> _repo;
    private readonly ITenantContext _tenantContext;

    public GetBranchesQueryHandler(IRepository<Branch> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var branches = await _repo.FindAsync(b => b.TenantId == tenantId, cancellationToken);
        return branches.Select(b => new BranchDto(b.BranchId, b.TenantId, b.BranchCode, b.BranchName,
            b.Address, b.City, b.StateCode, b.StateName,
            b.GSTIN, b.ContactPerson, b.ContactPhone, b.ContactEmail, b.IsActive));
    }
}
