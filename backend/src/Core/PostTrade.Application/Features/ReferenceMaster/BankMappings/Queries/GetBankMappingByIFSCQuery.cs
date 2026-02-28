using MediatR;
using PostTrade.Application.Features.ReferenceMaster.BankMappings.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.BankMappings.Queries;

public record GetBankMappingByIFSCQuery(string IFSCCode) : IRequest<BankMappingDto?>;

public class GetBankMappingByIFSCQueryHandler : IRequestHandler<GetBankMappingByIFSCQuery, BankMappingDto?>
{
    private readonly IRepository<BankMapping> _repo;

    public GetBankMappingByIFSCQueryHandler(IRepository<BankMapping> repo)
    {
        _repo = repo;
    }

    public async Task<BankMappingDto?> Handle(GetBankMappingByIFSCQuery request, CancellationToken cancellationToken)
    {
        var results = await _repo.FindAsync(
            m => m.IFSCCode == request.IFSCCode,
            cancellationToken);

        var m = results.FirstOrDefault();
        if (m is null) return null;
        return new BankMappingDto(m.MappingId, m.BankCode, m.IFSCCode, m.MICRCode, m.IsActive);
    }
}
