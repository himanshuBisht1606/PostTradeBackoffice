using MediatR;
using PostTrade.Application.Features.ReferenceMaster.PinCodes.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.PinCodes.Queries;

public record GetPinCodeByCodeQuery(string Code) : IRequest<PinCodeMasterDto?>;

public class GetPinCodeByCodeQueryHandler : IRequestHandler<GetPinCodeByCodeQuery, PinCodeMasterDto?>
{
    private readonly IRepository<PinCodeMaster> _repo;

    public GetPinCodeByCodeQueryHandler(IRepository<PinCodeMaster> repo)
    {
        _repo = repo;
    }

    public async Task<PinCodeMasterDto?> Handle(GetPinCodeByCodeQuery request, CancellationToken cancellationToken)
    {
        var results = await _repo.FindAsync(p => p.PinCode == request.Code, cancellationToken);
        var p = results.FirstOrDefault();
        if (p is null) return null;
        return new PinCodeMasterDto(p.PinCodeId, p.PinCode, p.District, p.City, p.StateCode, p.CountryCode, p.McxCode, p.IsActive);
    }
}
