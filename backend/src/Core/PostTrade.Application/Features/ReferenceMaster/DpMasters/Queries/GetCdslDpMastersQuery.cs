using MediatR;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.DpMasters.Queries;

public record GetCdslDpMastersQuery : IRequest<IEnumerable<CdslDpMasterDto>>;

public class GetCdslDpMastersQueryHandler : IRequestHandler<GetCdslDpMastersQuery, IEnumerable<CdslDpMasterDto>>
{
    private readonly IRepository<CdslDpMaster> _repo;

    public GetCdslDpMastersQueryHandler(IRepository<CdslDpMaster> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CdslDpMasterDto>> Handle(GetCdslDpMastersQuery request, CancellationToken cancellationToken)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        return items
            .OrderBy(d => d.DpCode)
            .Select(d => new CdslDpMasterDto(
                d.DpId, d.DpCode, d.DpName, d.SebiRegNo,
                d.City, d.State, d.PinCode, d.Phone, d.Email,
                d.MemberStatus, d.IsActive));
    }
}
