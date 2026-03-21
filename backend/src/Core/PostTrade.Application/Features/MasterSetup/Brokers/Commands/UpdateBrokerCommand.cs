using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Commands;

public record UpdateBrokerCommand(
    Guid BrokerId,

    // Identity
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
) : IRequest<BrokerDto?>;

public class UpdateBrokerCommandValidator : AbstractValidator<UpdateBrokerCommand>
{
    public UpdateBrokerCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
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
        broker.EntityType = request.EntityType;
        broker.Website = request.Website;
        broker.ContactEmail = request.ContactEmail;
        broker.ContactPhone = request.ContactPhone;
        broker.CIN = request.CIN;
        broker.TAN = request.TAN;
        broker.PAN = request.PAN;
        broker.GST = request.GST;
        broker.IncorporationDate = request.IncorporationDate;
        broker.RegisteredAddressLine1 = request.RegisteredAddressLine1;
        broker.RegisteredAddressLine2 = request.RegisteredAddressLine2;
        broker.RegisteredCity = request.RegisteredCity;
        broker.RegisteredState = request.RegisteredState;
        broker.RegisteredPinCode = request.RegisteredPinCode;
        broker.RegisteredCountry = request.RegisteredCountry ?? "India";
        broker.CorrespondenceSameAsRegistered = request.CorrespondenceSameAsRegistered;
        broker.CorrespondenceAddressLine1 = request.CorrespondenceAddressLine1;
        broker.CorrespondenceAddressLine2 = request.CorrespondenceAddressLine2;
        broker.CorrespondenceCity = request.CorrespondenceCity;
        broker.CorrespondenceState = request.CorrespondenceState;
        broker.CorrespondencePinCode = request.CorrespondencePinCode;
        broker.SEBIRegistrationNo = request.SEBIRegistrationNo;
        broker.SEBIRegistrationDate = request.SEBIRegistrationDate;
        broker.SEBIRegistrationExpiry = request.SEBIRegistrationExpiry;
        broker.ComplianceOfficerName = request.ComplianceOfficerName;
        broker.ComplianceOfficerEmail = request.ComplianceOfficerEmail;
        broker.ComplianceOfficerPhone = request.ComplianceOfficerPhone;
        broker.PrincipalOfficerName = request.PrincipalOfficerName;
        broker.PrincipalOfficerEmail = request.PrincipalOfficerEmail;
        broker.PrincipalOfficerPhone = request.PrincipalOfficerPhone;
        broker.SettlementBankName = request.SettlementBankName;
        broker.SettlementBankAccountNo = request.SettlementBankAccountNo;
        broker.SettlementBankIfsc = request.SettlementBankIfsc;
        broker.SettlementBankBranch = request.SettlementBankBranch;

        await _repo.UpdateAsync(broker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BrokerDto(broker.BrokerId, broker.TenantId, broker.BrokerCode, broker.BrokerName,
            broker.EntityType, broker.Status, broker.SEBIRegistrationNo,
            broker.ContactEmail, broker.ContactPhone,
            broker.RegisteredCity, broker.RegisteredState, broker.PAN, broker.GST);
    }
}
