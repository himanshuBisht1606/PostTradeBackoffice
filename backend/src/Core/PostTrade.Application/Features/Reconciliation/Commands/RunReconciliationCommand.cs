using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Reconciliation.DTOs;
using PostTrade.Application.Features.Reconciliation.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;
using ReconEntity = PostTrade.Domain.Entities.Reconciliation.Reconciliation;

namespace PostTrade.Application.Features.Reconciliation.Commands;

public record RunReconciliationCommand(
    DateTime ReconDate,
    string SettlementNo,
    ReconType ReconType,
    decimal SystemValue,
    decimal ExchangeValue,
    decimal ToleranceLimit,
    string? Comments
) : IRequest<ReconciliationDto>;

public class RunReconciliationCommandValidator : AbstractValidator<RunReconciliationCommand>
{
    public RunReconciliationCommandValidator()
    {
        RuleFor(x => x.ReconDate).NotEmpty();
        RuleFor(x => x.SettlementNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SystemValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExchangeValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ToleranceLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Comments).MaximumLength(500).When(x => x.Comments != null);
    }
}

public class RunReconciliationCommandHandler : IRequestHandler<RunReconciliationCommand, ReconciliationDto>
{
    private readonly IRepository<ReconEntity> _reconRepo;
    private readonly IRepository<ReconException> _exceptionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RunReconciliationCommandHandler(
        IRepository<ReconEntity> reconRepo,
        IRepository<ReconException> exceptionRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _reconRepo = reconRepo;
        _exceptionRepo = exceptionRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ReconciliationDto> Handle(RunReconciliationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var difference = Math.Abs(request.SystemValue - request.ExchangeValue);
        var status = difference <= request.ToleranceLimit
            ? ReconStatus.Matched
            : ReconStatus.Mismatched;

        var recon = new ReconEntity
        {
            ReconId = Guid.NewGuid(),
            TenantId = tenantId,
            ReconDate = request.ReconDate,
            SettlementNo = request.SettlementNo,
            ReconType = request.ReconType,
            SystemValue = request.SystemValue,
            ExchangeValue = request.ExchangeValue,
            Difference = difference,
            ToleranceLimit = request.ToleranceLimit,
            Status = status,
            Comments = request.Comments
        };

        await _reconRepo.AddAsync(recon, cancellationToken);

        if (status == ReconStatus.Mismatched)
        {
            var exception = new ReconException
            {
                ExceptionId = Guid.NewGuid(),
                ReconId = recon.ReconId,
                TenantId = tenantId,
                ExceptionType = ExceptionType.Other,
                ExceptionDescription = $"Reconciliation mismatch: system={request.SystemValue}, exchange={request.ExchangeValue}, diff={difference}",
                ReferenceNo = request.SettlementNo,
                Amount = difference,
                Status = ExceptionStatus.Open
            };

            await _exceptionRepo.AddAsync(exception, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetReconciliationsQueryHandler.ToDto(recon);
    }
}
