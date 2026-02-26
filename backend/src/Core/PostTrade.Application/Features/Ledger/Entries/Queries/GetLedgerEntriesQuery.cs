using MediatR;
using PostTrade.Application.Features.Ledger.Entries.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Entries.Queries;

public record GetLedgerEntriesQuery(
    Guid? ClientId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    LedgerType? LedgerType = null,
    EntryType? EntryType = null
) : IRequest<IEnumerable<LedgerEntryDto>>;

public class GetLedgerEntriesQueryHandler : IRequestHandler<GetLedgerEntriesQuery, IEnumerable<LedgerEntryDto>>
{
    private readonly IRepository<LedgerEntry> _repo;
    private readonly ITenantContext _tenantContext;

    public GetLedgerEntriesQueryHandler(IRepository<LedgerEntry> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<LedgerEntryDto>> Handle(GetLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var entries = await _repo.FindAsync(
            e => e.TenantId == tenantId &&
                 (request.ClientId == null || e.ClientId == request.ClientId) &&
                 (request.FromDate == null || e.PostingDate >= request.FromDate) &&
                 (request.ToDate == null || e.PostingDate <= request.ToDate) &&
                 (request.LedgerType == null || e.LedgerType == request.LedgerType) &&
                 (request.EntryType == null || e.EntryType == request.EntryType),
            cancellationToken);

        return entries
            .OrderByDescending(e => e.PostingDate)
            .ThenByDescending(e => e.CreatedAt)
            .Select(ToDto);
    }

    internal static LedgerEntryDto ToDto(LedgerEntry e) => new(
        e.LedgerId, e.TenantId, e.BrokerId, e.ClientId,
        e.VoucherNo, e.PostingDate, e.ValueDate,
        e.LedgerType, e.EntryType,
        e.Debit, e.Credit, e.Balance,
        e.ReferenceType, e.ReferenceId,
        e.Narration, e.IsReversed, e.ReversalLedgerId);
}
