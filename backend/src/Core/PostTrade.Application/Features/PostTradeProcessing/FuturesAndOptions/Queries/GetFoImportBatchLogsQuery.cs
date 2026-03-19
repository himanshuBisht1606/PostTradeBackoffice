using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

public record GetFoImportBatchLogsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 50
) : IRequest<FoImportBatchLogsPagedDto>;

public class GetFoImportBatchLogsQueryHandler : IRequestHandler<GetFoImportBatchLogsQuery, FoImportBatchLogsPagedDto>
{
    private readonly IRepository<FoFileImportLog> _logRepo;

    public GetFoImportBatchLogsQueryHandler(IRepository<FoFileImportLog> logRepo)
    {
        _logRepo = logRepo;
    }

    public async Task<FoImportBatchLogsPagedDto> Handle(GetFoImportBatchLogsQuery request, CancellationToken cancellationToken)
    {
        var all = await _logRepo.FindAsync(l => l.BatchId == request.BatchId, cancellationToken);
        var ordered = all.OrderBy(l => l.RowNumber).ToList();

        // Distinct summary — group by (Level, Message), count occurrences
        var summary = ordered
            .GroupBy(l => new { l.Level, l.Message })
            .Select(g => new FoImportBatchLogSummaryItemDto(g.Key.Level, g.Key.Message, g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();

        // Paginated individual rows
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var page = Math.Max(1, request.Page);
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new FoImportBatchLogDto(l.LogId, l.BatchId, l.RowNumber, l.Level, l.Message, l.RawData))
            .ToList();

        return new FoImportBatchLogsPagedDto(summary, items, totalCount, page, pageSize);
    }
}
