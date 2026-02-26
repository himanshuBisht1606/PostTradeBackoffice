using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

// ─── Nested input records (mirror the frontend payload) ───────────────────────

public record OnboardPanData(
    string Pan,
    string ClientType);

public record OnboardBasicDetails(
    string FirstName,
    string LastName,
    string Dob,
    string Gender,
    string Occupation,
    string? Prefix = null,
    string? MiddleName = null,
    string? FatherSpouseName = null,
    string? MotherName = null,
    string? MaritalStatus = null,
    string? GrossAnnualIncome = null);

public record OnboardAddress(
    string PermanentLine1,
    string PermanentCity,
    string PermanentState,
    string PermanentCountry,
    string PermanentPinCode,
    bool SameAsPermanent,
    string? PermanentLine2 = null,
    string? CorrLine1 = null,
    string? CorrLine2 = null,
    string? CorrCity = null,
    string? CorrState = null,
    string? CorrCountry = null,
    string? CorrPinCode = null);

public record OnboardContact(
    string Mobile,
    string Email,
    string? AlternateMobile = null);

public record OnboardNominee(
    string NomineeName,
    string Relationship,
    decimal SharePercentage,
    string? Dob = null,
    string? NomineePan = null,
    string? NomineeMobile = null,
    string? NomineeEmail = null,
    string? NomineeAddress = null);

// ─── Command ──────────────────────────────────────────────────────────────────

public record OnboardClientCommand(
    OnboardPanData Pan,
    OnboardBasicDetails BasicDetails,
    OnboardAddress Address,
    OnboardContact Contact,
    OnboardNominee? Nominee = null,
    Guid? BrokerId = null,
    Guid? BranchId = null
) : IRequest<OnboardingResultDto>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class OnboardClientCommandValidator : AbstractValidator<OnboardClientCommand>
{
    public OnboardClientCommandValidator()
    {
        RuleFor(x => x.Pan.Pan)
            .NotEmpty()
            .MaximumLength(10)
            .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
            .WithMessage("Invalid PAN format — expected AAAAA9999A");

        RuleFor(x => x.BasicDetails.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasicDetails.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasicDetails.Gender).NotEmpty();
        RuleFor(x => x.BasicDetails.Occupation).NotEmpty();
        RuleFor(x => x.BasicDetails.Dob)
            .NotEmpty()
            .Must(d => DateOnly.TryParse(d, out _))
            .WithMessage("Invalid date of birth — expected YYYY-MM-DD");

        RuleFor(x => x.Address.PermanentLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address.PermanentCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.PermanentState).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.PermanentPinCode)
            .NotEmpty()
            .Matches(@"^[1-9][0-9]{5}$")
            .WithMessage("Invalid PIN code");

        RuleFor(x => x.Contact.Mobile)
            .NotEmpty()
            .Matches(@"^[6-9]\d{9}$")
            .WithMessage("Invalid mobile number — must be 10 digits starting with 6–9");

        RuleFor(x => x.Contact.Email)
            .NotEmpty()
            .EmailAddress();

        When(x => x.Nominee != null, () =>
        {
            RuleFor(x => x.Nominee!.NomineeName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Nominee!.Relationship).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Nominee!.SharePercentage)
                .InclusiveBetween(1, 100)
                .WithMessage("Share percentage must be between 1 and 100");
        });
    }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public class OnboardClientCommandHandler : IRequestHandler<OnboardClientCommand, OnboardingResultDto>
{
    private readonly IRepository<Client> _clientRepo;
    private readonly IRepository<ClientNominee> _nomineeRepo;
    private readonly IRepository<Broker> _brokerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public OnboardClientCommandHandler(
        IRepository<Client> clientRepo,
        IRepository<ClientNominee> nomineeRepo,
        IRepository<Broker> brokerRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _clientRepo = clientRepo;
        _nomineeRepo = nomineeRepo;
        _brokerRepo = brokerRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<OnboardingResultDto> Handle(
        OnboardClientCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var clientId = Guid.NewGuid();
        var clientCode = GenerateClientCode(clientId);

        // Resolve broker: use supplied BrokerId or fall back to the first active broker for this tenant
        var brokerId = (request.BrokerId == null || request.BrokerId == Guid.Empty)
            ? (Guid?)null
            : request.BrokerId;

        if (brokerId == null)
        {
            var brokers = await _brokerRepo.FindAsync(
                b => b.TenantId == tenantId && b.Status == BrokerStatus.Active,
                cancellationToken);
            var defaultBroker = brokers.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "No active broker found for this tenant. Please create a broker first.");
            brokerId = defaultBroker.BrokerId;
        }

        // Build full display name
        var nameParts = new[] { request.BasicDetails.Prefix, request.BasicDetails.FirstName,
                                request.BasicDetails.MiddleName, request.BasicDetails.LastName }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var clientName = string.Join(" ", nameParts);

        // Build permanent address string
        var permAddrParts = new[] { request.Address.PermanentLine1, request.Address.PermanentLine2,
                                    request.Address.PermanentCity }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var permanentAddress = string.Join(", ", permAddrParts);

        // Build correspondence address string
        string? correspondenceAddress = null;
        if (request.Address.SameAsPermanent)
        {
            correspondenceAddress = permanentAddress;
        }
        else if (!string.IsNullOrWhiteSpace(request.Address.CorrLine1))
        {
            var corrParts = new[] { request.Address.CorrLine1, request.Address.CorrLine2,
                                    request.Address.CorrCity }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            correspondenceAddress = string.Join(", ", corrParts);
        }

        // Parse date of birth
        DateOnly? dob = DateOnly.TryParse(request.BasicDetails.Dob, out var parsedDob) ? parsedDob : null;

        var client = new Client
        {
            ClientId     = clientId,
            TenantId     = tenantId,
            BrokerId     = brokerId.Value,
            BranchId     = request.BranchId,
            ClientCode   = clientCode,
            ClientName   = clientName,
            Email        = request.Contact.Email,
            Phone        = request.Contact.Mobile,
            ClientType   = MapClientType(request.Pan.ClientType),
            Status       = ClientStatus.Active,
            PAN          = request.Pan.Pan.ToUpperInvariant(),
            StateName    = request.Address.PermanentState,
            Address      = permanentAddress,
            City         = request.Address.PermanentCity,
            PinCode      = request.Address.PermanentPinCode,
            CorrespondenceAddress = correspondenceAddress,
            DateOfBirth       = dob,
            Gender            = request.BasicDetails.Gender,
            MaritalStatus     = request.BasicDetails.MaritalStatus,
            Occupation        = request.BasicDetails.Occupation,
            GrossAnnualIncome = request.BasicDetails.GrossAnnualIncome,
            FatherSpouseName  = request.BasicDetails.FatherSpouseName,
            MotherName        = request.BasicDetails.MotherName,
            AlternateMobile   = request.Contact.AlternateMobile,
            KYCStatus    = KYCStatus.Pending,
            RiskCategory = RiskCategory.Moderate,
        };

        await _clientRepo.AddAsync(client, cancellationToken);

        // Persist nominee if provided
        if (request.Nominee is { } n)
        {
            DateOnly? nomineeDob = DateOnly.TryParse(n.Dob, out var nd) ? nd : null;

            var nominee = new ClientNominee
            {
                NomineeId       = Guid.NewGuid(),
                ClientId        = clientId,
                TenantId        = tenantId,
                NomineeName     = n.NomineeName,
                Relationship    = n.Relationship,
                SharePercentage = n.SharePercentage,
                DateOfBirth     = nomineeDob,
                NomineePAN      = n.NomineePan,
                Mobile          = n.NomineeMobile,
                Email           = n.NomineeEmail,
                Address         = n.NomineeAddress,
            };

            await _nomineeRepo.AddAsync(nominee, cancellationToken);
        }

        // Single SaveChanges — both client + nominee committed atomically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OnboardingResultDto(clientId, clientCode);
    }

    // CLI + first 8 hex chars of the new GUID (11 chars total, unique per run)
    private static string GenerateClientCode(Guid id) =>
        $"CLI{id:N}"[..11].ToUpperInvariant();

    private static ClientType MapClientType(string raw) => raw switch
    {
        "Individual"         => ClientType.Individual,
        "HUF"                => ClientType.Individual,
        "AJP"                => ClientType.Individual,
        "Company"            => ClientType.Corporate,
        "Firm / Partnership" => ClientType.Corporate,
        "AOP / BOI"          => ClientType.Corporate,
        "Trust"              => ClientType.Corporate,
        "BOI"                => ClientType.Corporate,
        "Local Authority"    => ClientType.Corporate,
        "Government"         => ClientType.Corporate,
        _                    => ClientType.Individual,
    };
}
