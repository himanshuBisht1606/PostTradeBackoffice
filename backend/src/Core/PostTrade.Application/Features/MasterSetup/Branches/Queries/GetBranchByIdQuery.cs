using MediatR;
using PostTrade.Application.Features.MasterSetup.Branches.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Branches.Queries;

public record GetBranchByIdQuery(Guid BranchId) : IRequest<BranchDto?>;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchDto?>
{
    private readonly IRepository<Branch> _repo;
    private readonly ITenantContext _tenantContext;

    public GetBranchByIdQueryHandler(IRepository<Branch> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<BranchDto?> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(b => b.BranchId == request.BranchId && b.TenantId == tenantId, cancellationToken);
        var b = results.FirstOrDefault();
        if (b is null) return null;
        return new BranchDto(b.BranchId, b.TenantId, b.BranchCode, b.BranchName,
            b.Address, b.City, b.StateCode, b.StateName,
            b.GSTIN, b.ContactPerson, b.ContactPhone, b.ContactEmail, b.IsActive);
    }
}
