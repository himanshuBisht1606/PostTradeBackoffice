using MediatR;
using PostTrade.Application.Features.ReferenceMaster.DpMasters.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.DpMasters.Queries;

public record GetCdslDpMasterByIdQuery(Guid DpId) : IRequest<CdslDpMasterDto?>;

public class GetCdslDpMasterByIdQueryHandler : IRequestHandler<GetCdslDpMasterByIdQuery, CdslDpMasterDto?>
{
    private readonly IRepository<CdslDpMaster> _repo;

    public GetCdslDpMasterByIdQueryHandler(IRepository<CdslDpMaster> repo)
    {
        _repo = repo;
    }

    public async Task<CdslDpMasterDto?> Handle(GetCdslDpMasterByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _repo.GetByIdAsync(request.DpId, cancellationToken);
        if (d is null) return null;
        return new CdslDpMasterDto(
            d.DpId, d.DpCode, d.DpName, d.SebiRegNo,
            d.City, d.State, d.PinCode, d.Phone, d.Email,
            d.MemberStatus, d.IsActive);
    }
}
