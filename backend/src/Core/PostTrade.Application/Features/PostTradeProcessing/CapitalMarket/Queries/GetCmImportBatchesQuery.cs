using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Queries;

public record GetCmImportBatchesQuery(
    CmFileType? FileType,
    string? Exchange,
    DateOnly? TradingDate,
    CmImportStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<IEnumerable<CmImportBatchDto>>;

public class GetCmImportBatchesQueryHandler : IRequestHandler<GetCmImportBatchesQuery, IEnumerable<CmImportBatchDto>>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly ITenantContext _tenantContext;

    public GetCmImportBatchesQueryHandler(IRepository<CmFileImportBatch> batchRepo, ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CmImportBatchDto>> Handle(GetCmImportBatchesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var batches = await _batchRepo.FindAsync(b => b.TenantId == tenantId, cancellationToken);

        var query = batches.AsEnumerable();

        if (request.FileType.HasValue)
            query = query.Where(b => b.FileType == request.FileType.Value);

        if (!string.IsNullOrWhiteSpace(request.Exchange))
            query = query.Where(b => b.Exchange == request.Exchange);

        if (request.TradingDate.HasValue)
            query = query.Where(b => b.TradingDate == request.TradingDate.Value);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        return query
            .OrderByDescending(b => b.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new CmImportBatchDto(
                b.BatchId, b.TenantId, b.FileType, b.Exchange, b.TradingDate,
                b.Status, b.TriggerSource, b.FileName, b.TotalRows, b.CreatedRows,
                b.SkippedRows, b.ErrorRows, b.StartedAt, b.CompletedAt, b.ErrorMessage))
            .ToList();
    }
}
