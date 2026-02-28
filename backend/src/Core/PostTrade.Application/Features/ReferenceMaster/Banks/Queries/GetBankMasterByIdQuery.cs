using MediatR;
using PostTrade.Application.Features.ReferenceMaster.Banks.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.Banks.Queries;

public record GetBankMasterByIdQuery(Guid BankId) : IRequest<BankMasterDto?>;

public class GetBankMasterByIdQueryHandler : IRequestHandler<GetBankMasterByIdQuery, BankMasterDto?>
{
    private readonly IRepository<BankMaster> _repo;

    public GetBankMasterByIdQueryHandler(IRepository<BankMaster> repo)
    {
        _repo = repo;
    }

    public async Task<BankMasterDto?> Handle(GetBankMasterByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await _repo.GetByIdAsync(request.BankId, cancellationToken);
        if (b is null) return null;
        return new BankMasterDto(b.BankId, b.BankCode, b.BankName, b.IFSCPrefix, b.IsActive);
    }
}
