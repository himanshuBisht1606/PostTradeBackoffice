using MediatR;
using PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Queries;

public record GetFoImportBatchLogsQuery(Guid BatchId) : IRequest<IEnumerable<FoImportBatchLogDto>>;

public class GetFoImportBatchLogsQueryHandler : IRequestHandler<GetFoImportBatchLogsQuery, IEnumerable<FoImportBatchLogDto>>
{
    private readonly IRepository<FoFileImportLog> _logRepo;

    public GetFoImportBatchLogsQueryHandler(IRepository<FoFileImportLog> logRepo)
    {
        _logRepo = logRepo;
    }

    public async Task<IEnumerable<FoImportBatchLogDto>> Handle(GetFoImportBatchLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _logRepo.FindAsync(l => l.BatchId == request.BatchId, cancellationToken);
        return logs
            .OrderBy(l => l.RowNumber)
            .Select(l => new FoImportBatchLogDto(l.LogId, l.BatchId, l.RowNumber, l.Level, l.Message, l.RawData))
            .ToList();
    }
}
