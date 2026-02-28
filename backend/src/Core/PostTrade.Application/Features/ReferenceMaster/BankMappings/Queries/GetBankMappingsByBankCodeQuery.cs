using MediatR;
using PostTrade.Application.Features.ReferenceMaster.BankMappings.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.BankMappings.Queries;

public record GetBankMappingsByBankCodeQuery(string BankCode) : IRequest<IEnumerable<BankMappingDto>>;

public class GetBankMappingsByBankCodeQueryHandler : IRequestHandler<GetBankMappingsByBankCodeQuery, IEnumerable<BankMappingDto>>
{
    private readonly IRepository<BankMapping> _repo;

    public GetBankMappingsByBankCodeQueryHandler(IRepository<BankMapping> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<BankMappingDto>> Handle(GetBankMappingsByBankCodeQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BankCode))
            return [];

        var mappings = await _repo.FindAsync(
            m => m.BankCode == request.BankCode,
            cancellationToken);

        return mappings
            .Take(100)
            .Select(m => new BankMappingDto(m.MappingId, m.BankCode, m.IFSCCode, m.MICRCode, m.IsActive));
    }
}
