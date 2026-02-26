using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Tenants.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Tenants.Commands;

public record UpdateTenantCommand(
    Guid TenantId,
    string TenantName,
    string ContactEmail,
    string ContactPhone,
    TenantStatus Status,
    string? Address,
    string? City,
    string? Country,
    string? TaxId
) : IRequest<TenantDto?>;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantDto?>
{
    private readonly IRepository<Tenant> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTenantCommandHandler(IRepository<Tenant> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantDto?> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var results = await _repo.FindAsync(t => t.TenantId == request.TenantId, cancellationToken);
        var tenant = results.FirstOrDefault();
        if (tenant is null) return null;

        tenant.TenantName = request.TenantName;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;
        tenant.Status = request.Status;
        tenant.Address = request.Address;
        tenant.City = request.City;
        tenant.Country = request.Country;
        tenant.TaxId = request.TaxId;

        await _repo.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantDto(tenant.TenantId, tenant.TenantCode, tenant.TenantName,
            tenant.ContactEmail, tenant.ContactPhone, tenant.Status,
            tenant.Address, tenant.City, tenant.Country, tenant.TaxId, tenant.LicenseExpiryDate);
    }
}
