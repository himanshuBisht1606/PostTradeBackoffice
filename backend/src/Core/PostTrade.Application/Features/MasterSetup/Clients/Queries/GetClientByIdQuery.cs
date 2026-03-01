using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Clients.Queries;

public record GetClientByIdQuery(Guid ClientId) : IRequest<ClientDetailDto?>;

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDetailDto?>
{
    private readonly IRepository<Client> _repo;
    private readonly ITenantContext _tenantContext;

    public GetClientByIdQueryHandler(IRepository<Client> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<ClientDetailDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            c => c.ClientId == request.ClientId && c.TenantId == tenantId,
            cancellationToken);
        var c = results.FirstOrDefault();
        if (c is null) return null;
        return Map(c);
    }

    internal static ClientDetailDto Map(Client c) => new(
        c.ClientId, c.TenantId, c.BrokerId, c.BranchId,
        c.ClientCode, c.ClientName, c.Email, c.Phone, c.ClientType, c.Status,
        c.PAN, c.Aadhaar, c.DPId, c.DematAccountNo, c.Depository,
        c.Address, c.StateCode, c.StateName, c.BankAccountNo, c.BankName,
        c.BankIFSC, c.KYCStatus, c.RiskCategory,
        // Extended personal
        c.DateOfBirth?.ToString("yyyy-MM-dd"),
        c.Gender, c.MaritalStatus, c.Occupation, c.GrossAnnualIncome,
        c.FatherSpouseName, c.MotherName,
        // Extended contact & address
        c.AlternateMobile, c.City, c.PinCode, c.CorrespondenceAddress,
        // Extended identity
        c.HolderType,
        c.CitizenshipStatus, c.ResidentialStatus,
        // Extended bank & demat
        c.AccountType, c.BranchName);
}
