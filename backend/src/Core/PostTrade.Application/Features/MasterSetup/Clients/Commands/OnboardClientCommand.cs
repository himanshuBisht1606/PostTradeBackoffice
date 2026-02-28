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
    string ClientType,
    string HolderType = "Single");

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
    string? GrossAnnualIncome = null,
    string? CitizenshipStatus = null,
    string? ResidentialStatus = null,
    string? IdentityProofType = null,
    string? IdentityProofNumber = null);

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

public record OnboardBankDetails(
    string BankName,
    string BranchName,
    string AccountNumber,
    string IfscCode,
    string AccountType);

public record OnboardDematAccount(
    string Depository,
    string DpId,
    string ClientId,
    string DpName,
    List<string>? Segments = null);

public record OnboardFatca(
    string TaxCountry,
    bool IsUsPerson,
    string SourceOfWealth,
    string? Tin = null);

public record OnboardDeclaration(
    bool AcceptedTerms,
    bool InformationAccurate);

public record OnboardJointHolder(
    int HolderNumber,
    string Pan,
    string FirstName,
    string LastName,
    string Dob,
    string Relationship);

public record OnboardEntityDetails(
    string EntityName,
    string? RegistrationNumber = null,
    string? DateOfConstitution = null,
    string? ConstitutionType = null,
    string? GSTNumber = null,
    string? AnnualTurnover = null,
    string? KartaName = null,
    string? KartaPan = null);

public record OnboardAuthorizedSignatory(
    string Name,
    string Designation,
    string Pan,
    string? Din = null,
    string? Mobile = null,
    string? Email = null);

// ─── Command ──────────────────────────────────────────────────────────────────

public record OnboardClientCommand(
    OnboardPanData Pan,
    OnboardAddress Address,
    OnboardContact Contact,
    OnboardBasicDetails? BasicDetails = null,
    OnboardNominee? Nominee = null,
    OnboardBankDetails? BankDetails = null,
    OnboardDematAccount? DematAccount = null,
    OnboardFatca? Fatca = null,
    OnboardDeclaration? Declaration = null,
    List<OnboardJointHolder>? JointHolders = null,
    OnboardEntityDetails? EntityDetails = null,
    List<OnboardAuthorizedSignatory>? AuthorizedSignatories = null,
    Guid? BrokerId = null,
    Guid? BranchId = null
) : IRequest<OnboardingResultDto>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class OnboardClientCommandValidator : AbstractValidator<OnboardClientCommand>
{
    private static readonly HashSet<string> IndividualTypes = new(StringComparer.OrdinalIgnoreCase)
        { "Individual", "AJP" };

    public OnboardClientCommandValidator()
    {
        RuleFor(x => x.Pan.Pan)
            .NotEmpty()
            .MaximumLength(10)
            .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
            .WithMessage("Invalid PAN format — expected AAAAA9999A");

        // Individual clients require BasicDetails
        When(x => IndividualTypes.Contains(x.Pan.ClientType), () =>
        {
            RuleFor(x => x.BasicDetails).NotNull().WithMessage("BasicDetails required for individual clients");
            When(x => x.BasicDetails != null, () =>
            {
                RuleFor(x => x.BasicDetails!.FirstName).NotEmpty().MaximumLength(100);
                RuleFor(x => x.BasicDetails!.LastName).NotEmpty().MaximumLength(100);
                RuleFor(x => x.BasicDetails!.Gender).NotEmpty();
                RuleFor(x => x.BasicDetails!.Occupation).NotEmpty();
                RuleFor(x => x.BasicDetails!.Dob)
                    .NotEmpty()
                    .Must(d => DateOnly.TryParse(d, out _))
                    .WithMessage("Invalid date of birth — expected YYYY-MM-DD");
            });
        });

        // Non-individual clients require EntityDetails
        When(x => !IndividualTypes.Contains(x.Pan.ClientType), () =>
        {
            RuleFor(x => x.EntityDetails).NotNull().WithMessage("EntityDetails required for non-individual clients");
            When(x => x.EntityDetails != null, () =>
            {
                RuleFor(x => x.EntityDetails!.EntityName).NotEmpty().MaximumLength(200);
            });
            When(x => x.AuthorizedSignatories != null && x.AuthorizedSignatories.Count > 0, () =>
            {
                RuleForEach(x => x.AuthorizedSignatories).ChildRules(s =>
                {
                    s.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                    s.RuleFor(x => x.Designation).NotEmpty().MaximumLength(100);
                    s.RuleFor(x => x.Pan)
                        .NotEmpty()
                        .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
                        .WithMessage("Invalid signatory PAN format");
                });
            });
        });

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

        When(x => x.JointHolders != null && x.JointHolders.Count > 0, () =>
        {
            RuleForEach(x => x.JointHolders).ChildRules(h =>
            {
                h.RuleFor(x => x.Pan)
                    .NotEmpty()
                    .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
                    .WithMessage("Invalid joint holder PAN format");
                h.RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
                h.RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
                h.RuleFor(x => x.Relationship).NotEmpty().MaximumLength(50);
                h.RuleFor(x => x.HolderNumber).InclusiveBetween(2, 3);
            });
        });
    }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public class OnboardClientCommandHandler : IRequestHandler<OnboardClientCommand, OnboardingResultDto>
{
    private static readonly HashSet<string> IndividualTypes = new(StringComparer.OrdinalIgnoreCase)
        { "Individual", "AJP" };

    private readonly IRepository<Client> _clientRepo;
    private readonly IRepository<ClientNominee> _nomineeRepo;
    private readonly IRepository<ClientFatca> _fatcaRepo;
    private readonly IRepository<ClientJointHolder> _jointHolderRepo;
    private readonly IRepository<ClientAuthorizedSignatory> _signatoryRepo;
    private readonly IRepository<Broker> _brokerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public OnboardClientCommandHandler(
        IRepository<Client> clientRepo,
        IRepository<ClientNominee> nomineeRepo,
        IRepository<ClientFatca> fatcaRepo,
        IRepository<ClientJointHolder> jointHolderRepo,
        IRepository<ClientAuthorizedSignatory> signatoryRepo,
        IRepository<Broker> brokerRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _clientRepo = clientRepo;
        _nomineeRepo = nomineeRepo;
        _fatcaRepo = fatcaRepo;
        _jointHolderRepo = jointHolderRepo;
        _signatoryRepo = signatoryRepo;
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
        var isIndividual = IndividualTypes.Contains(request.Pan.ClientType);

        // Resolve broker
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

        // Build display name
        string clientName;
        if (!isIndividual && request.EntityDetails != null)
        {
            clientName = request.EntityDetails.EntityName;
        }
        else if (request.BasicDetails != null)
        {
            var nameParts = new[] { request.BasicDetails.Prefix, request.BasicDetails.FirstName,
                                    request.BasicDetails.MiddleName, request.BasicDetails.LastName }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            clientName = string.Join(" ", nameParts);
        }
        else
        {
            clientName = request.Pan.Pan;
        }

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

        // Parse date of birth (individual only)
        DateOnly? dob = request.BasicDetails?.Dob is string dobStr && DateOnly.TryParse(dobStr, out var parsedDob)
            ? parsedDob : null;

        // Parse date of constitution (non-individual)
        DateOnly? dateOfConstitution = request.EntityDetails?.DateOfConstitution is string docStr
            && DateOnly.TryParse(docStr, out var parsedDoc) ? parsedDoc : null;

        // Determine segments
        var segments = request.DematAccount?.Segments ?? new List<string>();
        var segmentNse = segments.Contains("NSE", StringComparer.OrdinalIgnoreCase);
        var segmentBse = segments.Contains("BSE", StringComparer.OrdinalIgnoreCase);
        var segmentMcx = segments.Contains("MCX", StringComparer.OrdinalIgnoreCase);

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
            Status       = ClientStatus.Registered,
            PAN          = request.Pan.Pan.ToUpperInvariant(),
            StateName    = request.Address.PermanentState,
            Address      = permanentAddress,
            City         = request.Address.PermanentCity,
            PinCode      = request.Address.PermanentPinCode,
            CorrespondenceAddress = correspondenceAddress,
            AlternateMobile   = request.Contact.AlternateMobile,
            KYCStatus    = KYCStatus.Pending,
            RiskCategory = RiskCategory.Moderate,

            // Individual fields
            DateOfBirth       = dob,
            Gender            = request.BasicDetails?.Gender,
            MaritalStatus     = request.BasicDetails?.MaritalStatus,
            Occupation        = request.BasicDetails?.Occupation,
            GrossAnnualIncome = request.BasicDetails?.GrossAnnualIncome,
            FatherSpouseName  = request.BasicDetails?.FatherSpouseName,
            MotherName        = request.BasicDetails?.MotherName,
            HolderType        = request.Pan.HolderType,
            CitizenshipStatus   = request.BasicDetails?.CitizenshipStatus,
            ResidentialStatus   = request.BasicDetails?.ResidentialStatus,
            IdentityProofType   = request.BasicDetails?.IdentityProofType,
            IdentityProofNumber = request.BasicDetails?.IdentityProofNumber,

            // Non-individual entity fields
            EntityRegistrationNumber = request.EntityDetails?.RegistrationNumber,
            DateOfConstitution       = dateOfConstitution,
            ConstitutionType         = request.EntityDetails?.ConstitutionType,
            GSTNumber                = request.EntityDetails?.GSTNumber,
            AnnualTurnover           = request.EntityDetails?.AnnualTurnover,
            KartaName                = request.EntityDetails?.KartaName,
            KartaPan                 = request.EntityDetails?.KartaPan,

            // Bank / demat
            AccountType    = request.BankDetails?.AccountType,
            BranchName     = request.BankDetails?.BranchName,
            BankName       = request.BankDetails?.BankName,
            BankAccountNo  = request.BankDetails?.AccountNumber,
            BankIFSC       = request.BankDetails?.IfscCode,
            DPId           = request.DematAccount?.DpId,
            DematAccountNo = request.DematAccount?.ClientId,
            Depository     = request.DematAccount?.Depository != null
                                ? Enum.TryParse<Depository>(request.DematAccount.Depository, out var dep)
                                    ? dep : (Depository?)null
                                : null,
            SegmentNSE = segmentNse,
            SegmentBSE = segmentBse,
            SegmentMCX = segmentMcx,
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

        // Persist joint holders if provided
        if (request.JointHolders is { Count: > 0 } jointHolders)
        {
            foreach (var jh in jointHolders)
            {
                DateOnly.TryParse(jh.Dob, out var jhDob);
                var jointHolder = new ClientJointHolder
                {
                    JointHolderId = Guid.NewGuid(),
                    ClientId      = clientId,
                    TenantId      = tenantId,
                    HolderNumber  = jh.HolderNumber,
                    Pan           = jh.Pan.ToUpperInvariant(),
                    FirstName     = jh.FirstName,
                    LastName      = jh.LastName,
                    DateOfBirth   = jhDob,
                    Relationship  = jh.Relationship,
                };
                await _jointHolderRepo.AddAsync(jointHolder, cancellationToken);
            }
        }

        // Persist FATCA if provided
        if (request.Fatca is { } fatca)
        {
            var clientFatca = new ClientFatca
            {
                ClientFatcaId  = Guid.NewGuid(),
                ClientId       = clientId,
                TenantId       = tenantId,
                TaxCountry     = fatca.TaxCountry,
                Tin            = fatca.Tin,
                IsUsPerson     = fatca.IsUsPerson,
                SourceOfWealth = fatca.SourceOfWealth,
            };
            await _fatcaRepo.AddAsync(clientFatca, cancellationToken);
        }

        // Persist authorized signatories if provided
        if (request.AuthorizedSignatories is { Count: > 0 } signatories)
        {
            foreach (var s in signatories)
            {
                var signatory = new ClientAuthorizedSignatory
                {
                    SignatoryId = Guid.NewGuid(),
                    ClientId    = clientId,
                    TenantId    = tenantId,
                    Name        = s.Name,
                    Designation = s.Designation,
                    Pan         = s.Pan.ToUpperInvariant(),
                    Din         = s.Din,
                    Mobile      = s.Mobile,
                    Email       = s.Email,
                };
                await _signatoryRepo.AddAsync(signatory, cancellationToken);
            }
        }

        // Single SaveChanges — all entities committed atomically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OnboardingResultDto(clientId, clientCode);
    }

    private static string GenerateClientCode(Guid id) =>
        $"CLI{id:N}"[..11].ToUpperInvariant();

    private static ClientType MapClientType(string raw) => raw switch
    {
        "Individual"         => ClientType.Individual,
        "AJP"                => ClientType.Individual,
        "HUF"                => ClientType.Corporate,
        "Company"            => ClientType.Corporate,
        "Firm"               => ClientType.Corporate,
        "Firm / Partnership" => ClientType.Corporate,
        "AOP"                => ClientType.Corporate,
        "AOP / BOI"          => ClientType.Corporate,
        "Trust"              => ClientType.Corporate,
        "BOI"                => ClientType.Corporate,
        "Local Authority"    => ClientType.Corporate,
        "Government"         => ClientType.Corporate,
        _                    => ClientType.Individual,
    };
}
