using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Reconciliation.DTOs;
using PostTrade.Application.Features.Reconciliation.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Reconciliation.Commands;

public record ResolveReconExceptionCommand(
    Guid ExceptionId,
    string Resolution
) : IRequest<ReconExceptionDto?>;

public class ResolveReconExceptionCommandValidator : AbstractValidator<ResolveReconExceptionCommand>
{
    public ResolveReconExceptionCommandValidator()
    {
        RuleFor(x => x.ExceptionId).NotEmpty();
        RuleFor(x => x.Resolution).NotEmpty().MaximumLength(500);
    }
}

public class ResolveReconExceptionCommandHandler : IRequestHandler<ResolveReconExceptionCommand, ReconExceptionDto?>
{
    private readonly IRepository<ReconException> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ResolveReconExceptionCommandHandler(
        IRepository<ReconException> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ReconExceptionDto?> Handle(ResolveReconExceptionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var results = await _repo.FindAsync(
            e => e.ExceptionId == request.ExceptionId && e.TenantId == tenantId,
            cancellationToken);

        var exception = results.FirstOrDefault();
        if (exception is null) return null;

        if (exception.Status == ExceptionStatus.Resolved || exception.Status == ExceptionStatus.Closed)
            throw new InvalidOperationException($"Exception is already in '{exception.Status}' status.");

        exception.Status = ExceptionStatus.Resolved;
        exception.Resolution = request.Resolution;
        exception.ResolvedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(exception, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetReconExceptionsQueryHandler.ToDto(exception);
    }
}
