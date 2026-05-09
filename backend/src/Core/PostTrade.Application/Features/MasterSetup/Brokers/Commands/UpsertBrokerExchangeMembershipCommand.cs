using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

/// <summary>
/// Creates or updates a broker's membership for a given ExchangeSegment.
/// If a membership already exists for (BrokerId, ExchangeSegmentId) it is updated; otherwise a new record is created.
/// </summary>
public record UpsertBrokerExchangeMembershipCommand(
    Guid BrokerId,
    Guid ExchangeSegmentId,
    string TradingMemberId,
    string? ClearingMemberId,
    MembershipType MembershipType,
    DateOnly EffectiveDate,
    DateOnly? ExpiryDate,
    bool IsActive
) : IRequest<BrokerExchangeMembershipDto>;

public class UpsertBrokerExchangeMembershipCommandValidator : AbstractValidator<UpsertBrokerExchangeMembershipCommand>
{
    public UpsertBrokerExchangeMembershipCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
        RuleFor(x => x.ExchangeSegmentId).NotEmpty();
        RuleFor(x => x.TradingMemberId).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ClearingMemberId).MaximumLength(20).When(x => x.ClearingMemberId != null);
        RuleFor(x => x.MembershipType).IsInEnum();
        RuleFor(x => x.ExpiryDate).GreaterThan(x => x.EffectiveDate)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be after effective date");
    }
}

public class UpsertBrokerExchangeMembershipCommandHandler
    : IRequestHandler<UpsertBrokerExchangeMembershipCommand, BrokerExchangeMembershipDto>
{
    private readonly IRepository<BrokerExchangeMembership> _repo;
    private readonly IRepository<ExchangeSegment> _esRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpsertBrokerExchangeMembershipCommandHandler(
        IRepository<BrokerExchangeMembership> repo,
        IRepository<ExchangeSegment> esRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _esRepo = esRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<BrokerExchangeMembershipDto> Handle(
        UpsertBrokerExchangeMembershipCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var existing = (await _repo.FindAsync(
            m => m.BrokerId == request.BrokerId && m.ExchangeSegmentId == request.ExchangeSegmentId,
            cancellationToken)).FirstOrDefault();

        BrokerExchangeMembership membership;
        if (existing is null)
        {
            membership = new BrokerExchangeMembership
            {
                BrokerExchangeMembershipId = Guid.NewGuid(),
                BrokerId = request.BrokerId,
                TenantId = tenantId,
                ExchangeSegmentId = request.ExchangeSegmentId,
            };
            await _repo.AddAsync(membership, cancellationToken);
        }
        else
        {
            membership = existing;
        }

        membership.TradingMemberId = request.TradingMemberId;
        membership.ClearingMemberId = request.ClearingMemberId;
        membership.MembershipType = request.MembershipType;
        membership.EffectiveDate = request.EffectiveDate;
        membership.ExpiryDate = request.ExpiryDate;
        membership.IsActive = request.IsActive;

        if (existing is not null)
            await _repo.UpdateAsync(membership, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var esResults = await _esRepo.FindAsync(es => es.ExchangeSegmentId == request.ExchangeSegmentId, cancellationToken);
        var es = esResults.FirstOrDefault();

        return new BrokerExchangeMembershipDto(
            membership.BrokerExchangeMembershipId, membership.BrokerId, membership.ExchangeSegmentId,
            es?.ExchangeSegmentCode ?? string.Empty,
            es?.ExchangeSegmentName ?? string.Empty,
            membership.TradingMemberId, membership.ClearingMemberId, membership.MembershipType,
            membership.EffectiveDate, membership.ExpiryDate, membership.IsActive);
    }
}
