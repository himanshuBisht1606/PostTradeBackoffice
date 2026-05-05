using MediatR;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.DpMasters.Queries;

public record GetNsdlDpMastersQuery : IRequest<IEnumerable<NsdlDpMasterDto>>;

public class GetNsdlDpMastersQueryHandler : IRequestHandler<GetNsdlDpMastersQuery, IEnumerable<NsdlDpMasterDto>>
{
    private readonly IRepository<NsdlDpMaster> _repo;

    public GetNsdlDpMastersQueryHandler(IRepository<NsdlDpMaster> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<NsdlDpMasterDto>> Handle(GetNsdlDpMastersQuery request, CancellationToken cancellationToken)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        return items
            .OrderBy(d => d.DpCode)
            .Select(d => new NsdlDpMasterDto(
                d.DpId, d.DpCode, d.DpName, d.SebiRegNo,
                d.City, d.State, d.PinCode, d.Phone, d.Email,
                d.MemberStatus, d.IsActive));
    }
}
