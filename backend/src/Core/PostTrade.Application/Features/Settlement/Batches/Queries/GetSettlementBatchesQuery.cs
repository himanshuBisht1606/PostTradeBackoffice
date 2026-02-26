using MediatR;
using PostTrade.Application.Features.Settlement.Batches.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Batches.Queries;

public record GetSettlementBatchesQuery(SettlementStatus? Status = null) : IRequest<IEnumerable<SettlementBatchDto>>;

public class GetSettlementBatchesQueryHandler : IRequestHandler<GetSettlementBatchesQuery, IEnumerable<SettlementBatchDto>>
{
    private readonly IRepository<SettlementBatch> _repo;
    private readonly ITenantContext _tenantContext;

    public GetSettlementBatchesQueryHandler(IRepository<SettlementBatch> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<SettlementBatchDto>> Handle(GetSettlementBatchesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var batches = await _repo.FindAsync(
            b => b.TenantId == tenantId &&
                 (request.Status == null || b.Status == request.Status),
            cancellationToken);

        return batches
            .OrderByDescending(b => b.SettlementDate)
            .Select(ToDto);
    }

    internal static SettlementBatchDto ToDto(SettlementBatch b) => new(
        b.BatchId, b.TenantId, b.SettlementNo, b.TradeDate, b.SettlementDate,
        b.ExchangeId, b.Status, b.TotalTrades, b.TotalTurnover, b.ProcessedAt, b.ProcessedBy);
}
