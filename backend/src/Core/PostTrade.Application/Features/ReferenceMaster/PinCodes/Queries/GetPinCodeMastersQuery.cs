using MediatR;
using PostTrade.Application.Features.ReferenceMaster.PinCodes.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.PinCodes.Queries;

public record GetPinCodeMastersQuery(string? StateCode = null) : IRequest<IEnumerable<PinCodeMasterDto>>;

public class GetPinCodeMastersQueryHandler : IRequestHandler<GetPinCodeMastersQuery, IEnumerable<PinCodeMasterDto>>
{
    private readonly IRepository<PinCodeMaster> _repo;

    public GetPinCodeMastersQueryHandler(IRepository<PinCodeMaster> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<PinCodeMasterDto>> Handle(GetPinCodeMastersQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<PinCodeMaster> all;

        if (!string.IsNullOrWhiteSpace(request.StateCode))
            all = await _repo.FindAsync(p => p.StateCode == request.StateCode, cancellationToken);
        else
            all = await _repo.GetAllAsync(cancellationToken);

        return all
            .OrderBy(p => p.PinCode)
            .Select(p => new PinCodeMasterDto(p.PinCodeId, p.PinCode, p.District, p.City, p.StateCode, p.CountryCode, p.McxCode, p.IsActive));
    }
}
