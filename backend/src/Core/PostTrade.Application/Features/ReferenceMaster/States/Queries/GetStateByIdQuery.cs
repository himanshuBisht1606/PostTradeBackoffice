using MediatR;
using PostTrade.Application.Features.ReferenceMaster.States.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.States.Queries;

public record GetStateByIdQuery(Guid StateId) : IRequest<StateMasterDto?>;

public class GetStateByIdQueryHandler : IRequestHandler<GetStateByIdQuery, StateMasterDto?>
{
    private readonly IRepository<StateMaster> _repo;

    public GetStateByIdQueryHandler(IRepository<StateMaster> repo)
    {
        _repo = repo;
    }

    public async Task<StateMasterDto?> Handle(GetStateByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _repo.GetByIdAsync(request.StateId, cancellationToken);
        if (s is null) return null;
        return new StateMasterDto(
            s.StateId, s.CountryId, s.StateCode, s.StateName,
            s.NseCode, s.BseName, s.CvlCode, s.NdmlCode,
            s.NcdexCode, s.NseKraCode, s.NsdlCode, s.IsActive);
    }
}
