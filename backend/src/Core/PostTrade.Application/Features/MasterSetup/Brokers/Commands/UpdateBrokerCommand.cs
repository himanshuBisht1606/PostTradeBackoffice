using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

public record UpdateBrokerCommand(
    Guid BrokerId,
    string BrokerName,
    string ContactEmail,
    string ContactPhone,
    BrokerStatus Status,
    string? SEBIRegistrationNo,
    string? Address,
    string? PAN,
    string? GST
) : IRequest<BrokerDto?>;

public class UpdateBrokerCommandValidator : AbstractValidator<UpdateBrokerCommand>
{
    public UpdateBrokerCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
        RuleFor(x => x.BrokerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
    }
}

public class UpdateBrokerCommandHandler : IRequestHandler<UpdateBrokerCommand, BrokerDto?>
{
    private readonly IRepository<Broker> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateBrokerCommandHandler(IRepository<Broker> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<BrokerDto?> Handle(UpdateBrokerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(b => b.BrokerId == request.BrokerId && b.TenantId == tenantId, cancellationToken);
        var broker = results.FirstOrDefault();
        if (broker is null) return null;

        broker.BrokerName = request.BrokerName;
        broker.ContactEmail = request.ContactEmail;
        broker.ContactPhone = request.ContactPhone;
        broker.Status = request.Status;
        broker.SEBIRegistrationNo = request.SEBIRegistrationNo;
        broker.Address = request.Address;
        broker.PAN = request.PAN;
        broker.GST = request.GST;

        await _repo.UpdateAsync(broker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BrokerDto(broker.BrokerId, broker.TenantId, broker.BrokerCode, broker.BrokerName,
            broker.SEBIRegistrationNo, broker.ContactEmail, broker.ContactPhone,
            broker.Status, broker.Address, broker.PAN, broker.GST);
    }
}
