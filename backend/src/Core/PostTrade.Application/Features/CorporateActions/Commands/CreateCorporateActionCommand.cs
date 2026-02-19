using FluentValidation;
using MediatR;
using PostTrade.Application.Features.CorporateActions.DTOs;
using PostTrade.Application.Features.CorporateActions.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.CorporateActions.Commands;

public record CreateCorporateActionCommand(
    Guid InstrumentId,
    CorporateActionType ActionType,
    DateTime AnnouncementDate,
    DateTime ExDate,
    DateTime RecordDate,
    DateTime? PaymentDate,
    decimal? DividendAmount,
    decimal? BonusRatio,
    decimal? SplitRatio,
    decimal? RightsRatio,
    decimal? RightsPrice
) : IRequest<CorporateActionDto>;

public class CreateCorporateActionCommandValidator : AbstractValidator<CreateCorporateActionCommand>
{
    public CreateCorporateActionCommandValidator()
    {
        RuleFor(x => x.InstrumentId).NotEmpty();
        RuleFor(x => x.AnnouncementDate).NotEmpty();
        RuleFor(x => x.ExDate).NotEmpty().GreaterThanOrEqualTo(x => x.AnnouncementDate);
        RuleFor(x => x.RecordDate).NotEmpty().GreaterThanOrEqualTo(x => x.ExDate);
        RuleFor(x => x.PaymentDate)
            .GreaterThanOrEqualTo(x => x.RecordDate)
            .When(x => x.PaymentDate.HasValue)
            .WithMessage("PaymentDate must be on or after RecordDate.");

        RuleFor(x => x.DividendAmount).GreaterThan(0)
            .When(x => x.ActionType == CorporateActionType.Dividend)
            .WithMessage("DividendAmount is required for Dividend actions.");

        RuleFor(x => x.BonusRatio).GreaterThan(0)
            .When(x => x.ActionType == CorporateActionType.Bonus)
            .WithMessage("BonusRatio is required for Bonus actions.");

        RuleFor(x => x.SplitRatio).GreaterThan(0)
            .When(x => x.ActionType == CorporateActionType.Split)
            .WithMessage("SplitRatio is required for Split actions.");

        RuleFor(x => x.RightsRatio).GreaterThan(0)
            .When(x => x.ActionType == CorporateActionType.Rights)
            .WithMessage("RightsRatio is required for Rights actions.");

        RuleFor(x => x.RightsPrice).GreaterThan(0)
            .When(x => x.ActionType == CorporateActionType.Rights)
            .WithMessage("RightsPrice is required for Rights actions.");
    }
}

public class CreateCorporateActionCommandHandler : IRequestHandler<CreateCorporateActionCommand, CorporateActionDto>
{
    private readonly IRepository<CorporateAction> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateCorporateActionCommandHandler(
        IRepository<CorporateAction> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<CorporateActionDto> Handle(CreateCorporateActionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var action = new CorporateAction
        {
            CorporateActionId = Guid.NewGuid(),
            TenantId = tenantId,
            InstrumentId = request.InstrumentId,
            ActionType = request.ActionType,
            AnnouncementDate = request.AnnouncementDate,
            ExDate = request.ExDate,
            RecordDate = request.RecordDate,
            PaymentDate = request.PaymentDate,
            DividendAmount = request.DividendAmount,
            BonusRatio = request.BonusRatio,
            SplitRatio = request.SplitRatio,
            RightsRatio = request.RightsRatio,
            RightsPrice = request.RightsPrice,
            Status = CorporateActionStatus.Announced,
            IsProcessed = false
        };

        await _repo.AddAsync(action, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetCorporateActionsQueryHandler.ToDto(action);
    }
}
