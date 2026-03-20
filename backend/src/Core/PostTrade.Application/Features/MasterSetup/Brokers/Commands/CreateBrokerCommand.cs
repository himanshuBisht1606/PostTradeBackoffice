using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

public record CreateBrokerCommand(
    // Identity
    string BrokerCode,
    string BrokerName,
    BrokerEntityType EntityType,
    string? Website,

    // Contact
    string ContactEmail,
    string ContactPhone,

    // Company / Corporate
    string? CIN,
    string? TAN,
    string? PAN,
    string? GST,
    DateOnly? IncorporationDate,

    // Registered Address
    string? RegisteredAddressLine1,
    string? RegisteredAddressLine2,
    string? RegisteredCity,
    string? RegisteredState,
    string? RegisteredPinCode,
    string? RegisteredCountry,

    // Correspondence Address
    bool CorrespondenceSameAsRegistered,
    string? CorrespondenceAddressLine1,
    string? CorrespondenceAddressLine2,
    string? CorrespondenceCity,
    string? CorrespondenceState,
    string? CorrespondencePinCode,

    // SEBI
    string? SEBIRegistrationNo,
    DateOnly? SEBIRegistrationDate,
    DateOnly? SEBIRegistrationExpiry,

    // Compliance
    string? ComplianceOfficerName,
    string? ComplianceOfficerEmail,
    string? ComplianceOfficerPhone,
    string? PrincipalOfficerName,
    string? PrincipalOfficerEmail,
    string? PrincipalOfficerPhone,

    // Settlement Bank
    string? SettlementBankName,
    string? SettlementBankAccountNo,
    string? SettlementBankIfsc,
    string? SettlementBankBranch
) : IRequest<BrokerDto>;

public class CreateBrokerCommandValidator : AbstractValidator<CreateBrokerCommand>
{
    public CreateBrokerCommandValidator()
    {
        RuleFor(x => x.BrokerCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BrokerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.EntityType).IsInEnum();
        RuleFor(x => x.PAN).Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]$").When(x => x.PAN != null)
            .WithMessage("PAN must be in format AAAAA9999A");
        RuleFor(x => x.GST).Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$").When(x => x.GST != null)
            .WithMessage("Invalid GST number format");
        RuleFor(x => x.SettlementBankIfsc).Matches(@"^[A-Z]{4}0[A-Z0-9]{6}$").When(x => x.SettlementBankIfsc != null)
            .WithMessage("IFSC must be 11 characters (e.g. HDFC0001234)");
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
            EntityType = request.EntityType,
            Status = BrokerStatus.Active,
            Website = request.Website,
            CIN = request.CIN,
            TAN = request.TAN,
            PAN = request.PAN,
            GST = request.GST,
            IncorporationDate = request.IncorporationDate,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            RegisteredAddressLine1 = request.RegisteredAddressLine1,
            RegisteredAddressLine2 = request.RegisteredAddressLine2,
            RegisteredCity = request.RegisteredCity,
            RegisteredState = request.RegisteredState,
            RegisteredPinCode = request.RegisteredPinCode,
            RegisteredCountry = request.RegisteredCountry ?? "India",
            CorrespondenceSameAsRegistered = request.CorrespondenceSameAsRegistered,
            CorrespondenceAddressLine1 = request.CorrespondenceAddressLine1,
            CorrespondenceAddressLine2 = request.CorrespondenceAddressLine2,
            CorrespondenceCity = request.CorrespondenceCity,
            CorrespondenceState = request.CorrespondenceState,
            CorrespondencePinCode = request.CorrespondencePinCode,
            SEBIRegistrationNo = request.SEBIRegistrationNo,
            SEBIRegistrationDate = request.SEBIRegistrationDate,
            SEBIRegistrationExpiry = request.SEBIRegistrationExpiry,
            ComplianceOfficerName = request.ComplianceOfficerName,
            ComplianceOfficerEmail = request.ComplianceOfficerEmail,
            ComplianceOfficerPhone = request.ComplianceOfficerPhone,
            PrincipalOfficerName = request.PrincipalOfficerName,
            PrincipalOfficerEmail = request.PrincipalOfficerEmail,
            PrincipalOfficerPhone = request.PrincipalOfficerPhone,
            SettlementBankName = request.SettlementBankName,
            SettlementBankAccountNo = request.SettlementBankAccountNo,
            SettlementBankIfsc = request.SettlementBankIfsc,
            SettlementBankBranch = request.SettlementBankBranch,
        };

        await _repo.AddAsync(broker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BrokerDto(broker.BrokerId, broker.TenantId, broker.BrokerCode, broker.BrokerName,
            broker.EntityType, broker.Status, broker.SEBIRegistrationNo,
            broker.ContactEmail, broker.ContactPhone,
            broker.RegisteredCity, broker.RegisteredState, broker.PAN, broker.GST);
    }
}
