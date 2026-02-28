using MediatR;
using PostTrade.Application.Features.ReferenceMaster.Banks.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.Banks.Queries;

public record GetBankMastersQuery : IRequest<IEnumerable<BankMasterDto>>;

public class GetBankMastersQueryHandler : IRequestHandler<GetBankMastersQuery, IEnumerable<BankMasterDto>>
{
    private readonly IRepository<BankMaster> _repo;

    public GetBankMastersQueryHandler(IRepository<BankMaster> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<BankMasterDto>> Handle(GetBankMastersQuery request, CancellationToken cancellationToken)
    {
        var banks = await _repo.GetAllAsync(cancellationToken);
        return banks
            .OrderBy(b => b.BankCode)
            .Select(b => new BankMasterDto(b.BankId, b.BankCode, b.BankName, b.IFSCPrefix, b.IsActive));
    }
}
