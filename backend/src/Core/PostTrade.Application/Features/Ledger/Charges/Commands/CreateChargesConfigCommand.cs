using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Ledger.Charges.DTOs;
using PostTrade.Application.Features.Ledger.Charges.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Charges.Commands;

public record CreateChargesConfigCommand(
    Guid? BrokerId,
    string ChargeName,
    ChargeType ChargeType,
    TradeSegment Segment,
    ChargeApplicableTo ApplicableTo,
    CalculationType CalculationType,
    decimal Rate,
    decimal? MinAmount,
    decimal? MaxAmount,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string? Remarks
) : IRequest<ChargesConfigDto>;

public class CreateChargesConfigCommandValidator : AbstractValidator<CreateChargesConfigCommand>
{
    public CreateChargesConfigCommandValidator()
    {
        RuleFor(x => x.ChargeName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0).When(x => x.MinAmount.HasValue);
        RuleFor(x => x.MaxAmount).GreaterThanOrEqualTo(0).When(x => x.MaxAmount.HasValue);
        RuleFor(x => x)
            .Must(x => x.MaxAmount == null || x.MinAmount == null || x.MaxAmount >= x.MinAmount)
            .WithMessage("MaxAmount must be greater than or equal to MinAmount.");
        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be after EffectiveFrom.");
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks != null);
    }
}

public class CreateChargesConfigCommandHandler : IRequestHandler<CreateChargesConfigCommand, ChargesConfigDto>
{
    private readonly IRepository<ChargesConfiguration> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateChargesConfigCommandHandler(
        IRepository<ChargesConfiguration> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ChargesConfigDto> Handle(CreateChargesConfigCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var config = new ChargesConfiguration
        {
            ChargesConfigId = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerId = request.BrokerId,
            ChargeName = request.ChargeName,
            ChargeType = request.ChargeType,
            Segment = request.Segment,
            ApplicableTo = request.ApplicableTo,
            CalculationType = request.CalculationType,
            Rate = request.Rate,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            IsActive = true,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Remarks = request.Remarks,
        };

        await _repo.AddAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetChargesConfigQueryHandler.ToDto(config);
    }
}
