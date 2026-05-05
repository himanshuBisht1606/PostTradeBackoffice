using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Ledger.Charges.DTOs;
using PostTrade.Application.Features.Ledger.Charges.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Charges.Commands;

public record UpdateChargesConfigCommand(
    Guid ChargesConfigId,
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

public class UpdateChargesConfigCommandValidator : AbstractValidator<UpdateChargesConfigCommand>
{
    public UpdateChargesConfigCommandValidator()
    {
        RuleFor(x => x.ChargesConfigId).NotEmpty();
        RuleFor(x => x.ChargeName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0).When(x => x.MinAmount.HasValue);
        RuleFor(x => x.MaxAmount).GreaterThanOrEqualTo(0).When(x => x.MaxAmount.HasValue);
        RuleFor(x => x)
            .Must(x => x.MaxAmount == null || x.MinAmount == null || x.MaxAmount >= x.MinAmount)
            .WithMessage("MaxAmount must be >= MinAmount.");
        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be after EffectiveFrom.");
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks != null);
    }
}

public class UpdateChargesConfigCommandHandler : IRequestHandler<UpdateChargesConfigCommand, ChargesConfigDto>
{
    private readonly IRepository<ChargesConfiguration> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateChargesConfigCommandHandler(
        IRepository<ChargesConfiguration> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ChargesConfigDto> Handle(UpdateChargesConfigCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var config = await _repo.FirstOrDefaultAsync(
            c => c.ChargesConfigId == request.ChargesConfigId && c.TenantId == tenantId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Charges config {request.ChargesConfigId} not found.");

        config.ChargeName = request.ChargeName;
        config.ChargeType = request.ChargeType;
        config.Segment = request.Segment;
        config.ApplicableTo = request.ApplicableTo;
        config.CalculationType = request.CalculationType;
        config.Rate = request.Rate;
        config.MinAmount = request.MinAmount;
        config.MaxAmount = request.MaxAmount;
        config.EffectiveFrom = request.EffectiveFrom;
        config.EffectiveTo = request.EffectiveTo;
        config.Remarks = request.Remarks;

        await _repo.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetChargesConfigQueryHandler.ToDto(config);
    }
}
