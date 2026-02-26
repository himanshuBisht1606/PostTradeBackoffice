using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Tenants.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Tenants.Commands;

public record CreateTenantCommand(
    string TenantCode,
    string TenantName,
    string ContactEmail,
    string ContactPhone,
    string? Address,
    string? City,
    string? Country,
    string? TaxId
) : IRequest<TenantDto>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.TenantCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IRepository<Tenant> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandHandler(IRepository<Tenant> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            TenantCode = request.TenantCode,
            TenantName = request.TenantName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            TaxId = request.TaxId,
            Status = TenantStatus.Active
        };

        await _repo.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantDto(tenant.TenantId, tenant.TenantCode, tenant.TenantName,
            tenant.ContactEmail, tenant.ContactPhone, tenant.Status,
            tenant.Address, tenant.City, tenant.Country, tenant.TaxId, tenant.LicenseExpiryDate);
    }
}
