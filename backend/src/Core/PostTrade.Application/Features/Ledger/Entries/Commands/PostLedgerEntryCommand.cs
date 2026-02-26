using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Ledger.Entries.DTOs;
using PostTrade.Application.Features.Ledger.Entries.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Entries.Commands;

public record PostLedgerEntryCommand(
    Guid BrokerId,
    Guid ClientId,
    string VoucherNo,
    DateTime PostingDate,
    DateTime ValueDate,
    LedgerType LedgerType,
    EntryType EntryType,
    decimal Debit,
    decimal Credit,
    string ReferenceType,
    Guid ReferenceId,
    string? Narration
) : IRequest<LedgerEntryDto>;

public class PostLedgerEntryCommandValidator : AbstractValidator<PostLedgerEntryCommand>
{
    public PostLedgerEntryCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.VoucherNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PostingDate).NotEmpty();
        RuleFor(x => x.ValueDate).NotEmpty();
        RuleFor(x => x.ReferenceType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Narration).MaximumLength(500).When(x => x.Narration != null);

        RuleFor(x => x)
            .Must(x => x.Debit >= 0 && x.Credit >= 0)
            .WithMessage("Debit and Credit must be non-negative.");

        RuleFor(x => x)
            .Must(x => x.Debit > 0 || x.Credit > 0)
            .WithMessage("At least one of Debit or Credit must be greater than zero.");
    }
}

public class PostLedgerEntryCommandHandler : IRequestHandler<PostLedgerEntryCommand, LedgerEntryDto>
{
    private readonly IRepository<LedgerEntry> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public PostLedgerEntryCommandHandler(
        IRepository<LedgerEntry> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<LedgerEntryDto> Handle(PostLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Calculate running balance for this client's ledger
        var existingEntries = await _repo.FindAsync(
            e => e.TenantId == tenantId &&
                 e.ClientId == request.ClientId &&
                 e.LedgerType == request.LedgerType,
            cancellationToken);

        var lastBalance = existingEntries
            .OrderByDescending(e => e.PostingDate)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => e.Balance)
            .FirstOrDefault();

        var newBalance = lastBalance + request.Credit - request.Debit;

        var entry = new LedgerEntry
        {
            LedgerId = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerId = request.BrokerId,
            ClientId = request.ClientId,
            VoucherNo = request.VoucherNo,
            PostingDate = request.PostingDate,
            ValueDate = request.ValueDate,
            LedgerType = request.LedgerType,
            EntryType = request.EntryType,
            Debit = request.Debit,
            Credit = request.Credit,
            Balance = newBalance,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            Narration = request.Narration,
            IsReversed = false
        };

        await _repo.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetLedgerEntriesQueryHandler.ToDto(entry);
    }
}
