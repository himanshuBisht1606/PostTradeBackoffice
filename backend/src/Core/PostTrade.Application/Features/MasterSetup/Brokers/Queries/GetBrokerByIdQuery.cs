using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Queries;

public record GetBrokerByIdQuery(Guid BrokerId) : IRequest<BrokerDetailDto?>;

public class GetBrokerByIdQueryHandler : IRequestHandler<GetBrokerByIdQuery, BrokerDetailDto?>
{
    private readonly IRepository<Broker> _repo;
    private readonly IRepository<BrokerExchangeMembership> _membershipRepo;
    private readonly IRepository<ExchangeSegment> _esRepo;
    private readonly ITenantContext _tenantContext;

    public GetBrokerByIdQueryHandler(
        IRepository<Broker> repo,
        IRepository<BrokerExchangeMembership> membershipRepo,
        IRepository<ExchangeSegment> esRepo,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _membershipRepo = membershipRepo;
        _esRepo = esRepo;
        _tenantContext = tenantContext;
    }

    public async Task<BrokerDetailDto?> Handle(GetBrokerByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var results = await _repo.FindAsync(
            b => b.BrokerId == request.BrokerId && b.TenantId == tenantId,
            cancellationToken);
        var b = results.FirstOrDefault();
        if (b is null) return null;

        var memberships = await _membershipRepo.FindAsync(
            m => m.BrokerId == b.BrokerId && m.TenantId == tenantId,
            cancellationToken);

        var esIds = memberships.Select(m => m.ExchangeSegmentId).ToHashSet();
        var exchangeSegments = esIds.Count > 0
            ? await _esRepo.FindAsync(es => esIds.Contains(es.ExchangeSegmentId), cancellationToken)
            : [];

        var esLookup = exchangeSegments.ToDictionary(es => es.ExchangeSegmentId);

        var membershipDtos = memberships.Select(m =>
        {
            esLookup.TryGetValue(m.ExchangeSegmentId, out var es);
            return new BrokerExchangeMembershipDto(
                m.BrokerExchangeMembershipId, m.BrokerId, m.ExchangeSegmentId,
                es?.ExchangeSegmentCode ?? string.Empty,
                es?.ExchangeSegmentName ?? string.Empty,
                m.TradingMemberId, m.ClearingMemberId, m.MembershipType,
                m.EffectiveDate, m.ExpiryDate, m.IsActive);
        });

        return new BrokerDetailDto(
            b.BrokerId, b.TenantId,
            b.BrokerCode, b.BrokerName, b.EntityType, b.Status, b.LogoUrl, b.Website,
            b.CIN, b.TAN, b.PAN, b.GST, b.IncorporationDate,
            b.ContactEmail, b.ContactPhone,
            b.RegisteredAddressLine1, b.RegisteredAddressLine2, b.RegisteredCity,
            b.RegisteredState, b.RegisteredPinCode, b.RegisteredCountry,
            b.CorrespondenceSameAsRegistered,
            b.CorrespondenceAddressLine1, b.CorrespondenceAddressLine2,
            b.CorrespondenceCity, b.CorrespondenceState, b.CorrespondencePinCode,
            b.SEBIRegistrationNo, b.SEBIRegistrationDate, b.SEBIRegistrationExpiry,
            b.ComplianceOfficerName, b.ComplianceOfficerEmail, b.ComplianceOfficerPhone,
            b.PrincipalOfficerName, b.PrincipalOfficerEmail, b.PrincipalOfficerPhone,
            b.SettlementBankName, b.SettlementBankAccountNo, b.SettlementBankIfsc, b.SettlementBankBranch,
            membershipDtos);
    }
}
