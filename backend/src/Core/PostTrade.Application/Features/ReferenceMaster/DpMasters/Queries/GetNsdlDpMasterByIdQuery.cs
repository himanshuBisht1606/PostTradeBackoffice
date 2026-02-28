using MediatR;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.DpMasters.Queries;

public record GetNsdlDpMasterByIdQuery(Guid DpId) : IRequest<NsdlDpMasterDto?>;

public class GetNsdlDpMasterByIdQueryHandler : IRequestHandler<GetNsdlDpMasterByIdQuery, NsdlDpMasterDto?>
{
    private readonly IRepository<NsdlDpMaster> _repo;

    public GetNsdlDpMasterByIdQueryHandler(IRepository<NsdlDpMaster> repo)
    {
        _repo = repo;
    }

    public async Task<NsdlDpMasterDto?> Handle(GetNsdlDpMasterByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _repo.GetByIdAsync(request.DpId, cancellationToken);
        if (d is null) return null;
        return new NsdlDpMasterDto(
            d.DpId, d.DpCode, d.DpName, d.SebiRegNo,
            d.City, d.State, d.PinCode, d.Phone, d.Email,
            d.MemberStatus, d.IsActive);
    }
}
