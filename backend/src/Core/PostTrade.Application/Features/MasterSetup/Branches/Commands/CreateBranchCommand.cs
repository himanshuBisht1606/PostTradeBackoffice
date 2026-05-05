using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Branches.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Branches.Commands;

public record CreateBranchCommand(
    string BranchCode,
    string BranchName,
    string? Address,
    string? City,
    string StateCode,
    string StateName,
    string? GSTIN,
    string? ContactPerson,
    string? ContactPhone,
    string? ContactEmail
) : IRequest<BranchDto>;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.StateName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GSTIN).MaximumLength(15).When(x => x.GSTIN != null);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => x.ContactEmail != null);
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, BranchDto>
{
    private readonly IRepository<Branch> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateBranchCommandHandler(IRepository<Branch> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<BranchDto> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var branch = new Branch
        {
            BranchId = Guid.NewGuid(),
            TenantId = tenantId,
            BranchCode = request.BranchCode,
            BranchName = request.BranchName,
            Address = request.Address,
            City = request.City,
            StateCode = request.StateCode,
            StateName = request.StateName,
            GSTIN = request.GSTIN,
            ContactPerson = request.ContactPerson,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            IsActive = true
        };

        await _repo.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BranchDto(branch.BranchId, branch.TenantId, branch.BranchCode, branch.BranchName,
            branch.Address, branch.City, branch.StateCode, branch.StateName,
            branch.GSTIN, branch.ContactPerson, branch.ContactPhone, branch.ContactEmail, branch.IsActive);
    }
}
