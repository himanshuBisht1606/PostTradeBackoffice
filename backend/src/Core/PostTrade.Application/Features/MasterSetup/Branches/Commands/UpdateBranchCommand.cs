using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Branches.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Branches.Commands;

public record UpdateBranchCommand(
    Guid BranchId,
    string BranchName,
    string? Address,
    string? City,
    string StateCode,
    string StateName,
    string? GSTIN,
    string? ContactPerson,
    string? ContactPhone,
    string? ContactEmail,
    bool IsActive
) : IRequest<BranchDto?>;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.StateName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GSTIN).MaximumLength(15).When(x => x.GSTIN != null);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => x.ContactEmail != null);
    }
}

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, BranchDto?>
{
    private readonly IRepository<Branch> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateBranchCommandHandler(IRepository<Branch> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<BranchDto?> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(b => b.BranchId == request.BranchId && b.TenantId == tenantId, cancellationToken);
        var branch = results.FirstOrDefault();
        if (branch is null) return null;

        branch.BranchName = request.BranchName;
        branch.Address = request.Address;
        branch.City = request.City;
        branch.StateCode = request.StateCode;
        branch.StateName = request.StateName;
        branch.GSTIN = request.GSTIN;
        branch.ContactPerson = request.ContactPerson;
        branch.ContactPhone = request.ContactPhone;
        branch.ContactEmail = request.ContactEmail;
        branch.IsActive = request.IsActive;

        await _repo.UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BranchDto(branch.BranchId, branch.TenantId, branch.BranchCode, branch.BranchName,
            branch.Address, branch.City, branch.StateCode, branch.StateName,
            branch.GSTIN, branch.ContactPerson, branch.ContactPhone, branch.ContactEmail, branch.IsActive);
    }
}
