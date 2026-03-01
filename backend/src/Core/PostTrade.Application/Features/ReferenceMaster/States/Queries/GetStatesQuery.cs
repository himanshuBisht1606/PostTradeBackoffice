using MediatR;
using PostTrade.Application.Features.ReferenceMaster.States.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.States.Queries;

public record GetStatesQuery : IRequest<IEnumerable<StateMasterDto>>;

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, IEnumerable<StateMasterDto>>
{
    private readonly IRepository<StateMaster> _repo;

    public GetStatesQueryHandler(IRepository<StateMaster> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<StateMasterDto>> Handle(GetStatesQuery request, CancellationToken cancellationToken)
    {
        var states = await _repo.GetAllAsync(cancellationToken);
        return states
            .OrderBy(s => s.StateCode)
            .Select(s => new StateMasterDto(
                s.StateId, s.CountryId, s.StateCode, s.StateName,
                s.NseCode, s.BseName, s.CvlCode, s.NdmlCode,
                s.NcdexCode, s.NseKraCode, s.NsdlCode, s.IsActive));
    }
}
