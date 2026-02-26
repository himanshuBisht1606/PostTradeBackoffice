using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

public record CreateBrokerCommand(
    string BrokerCode,
    string BrokerName,
    string ContactEmail,
    string ContactPhone,
    string? SEBIRegistrationNo,
    string? Address,
    string? PAN,
    string? GST
) : IRequest<BrokerDto>;

public class CreateBrokerCommandValidator : AbstractValidator<CreateBrokerCommand>
{
    public CreateBrokerCommandValidator()
    {
        RuleFor(x => x.BrokerCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BrokerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
    }
}

public class CreateBrokerCommandHandler : IRequestHandler<CreateBrokerCommand, BrokerDto>
{
    private readonly IRepository<Broker> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateBrokerCommandHandler(IRepository<Broker> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<BrokerDto> Handle(CreateBrokerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var broker = new Broker
        {
            BrokerId = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerCode = request.BrokerCode,
            BrokerName = request.BrokerName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            SEBIRegistrationNo = request.SEBIRegistrationNo,
            Address = request.Address,
            PAN = request.PAN,
            GST = request.GST,
            Status = BrokerStatus.Active
        };

        await _repo.AddAsync(broker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BrokerDto(broker.BrokerId, broker.TenantId, broker.BrokerCode, broker.BrokerName,
            broker.SEBIRegistrationNo, broker.ContactEmail, broker.ContactPhone,
            broker.Status, broker.Address, broker.PAN, broker.GST);
    }
}
