using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Roles.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Roles.Commands;

public record CreateRoleCommand(string RoleName, string? Description) : IRequest<RoleDto>;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(100);
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IRepository<Role> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateRoleCommandHandler(IRepository<Role> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            TenantId = tenantId,
            RoleName = request.RoleName,
            Description = request.Description,
            IsSystemRole = false
        };

        await _repo.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.RoleId, role.TenantId, role.RoleName, role.Description, role.IsSystemRole);
    }
}
