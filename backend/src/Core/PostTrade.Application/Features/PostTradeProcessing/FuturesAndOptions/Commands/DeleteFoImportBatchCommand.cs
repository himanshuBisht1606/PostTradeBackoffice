using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record DeleteFoImportBatchCommand(Guid BatchId) : IRequest<bool>;

public class DeleteFoImportBatchCommandHandler : IRequestHandler<DeleteFoImportBatchCommand, bool>
{
    private readonly IRepository<FoFileImportBatch> _batchRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFoImportBatchCommandHandler(IRepository<FoFileImportBatch> batchRepo, IUnitOfWork unitOfWork)
    {
        _batchRepo = batchRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteFoImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _batchRepo.GetByIdAsync(request.BatchId, cancellationToken);
        if (batch == null) return false;

        // Hard-delete the batch; cascade deletes logs and all linked data rows
        await _unitOfWork.ExecuteDeleteAsync<FoTrade>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<FoBhavCopy>(b => b.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<FoStt>(s => s.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<FoStampDuty>(s => s.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<FoPosition>(p => p.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<FoContractMaster>(c => c.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<FoFileImportLog>(l => l.BatchId == request.BatchId, cancellationToken);
        await _batchRepo.DeleteAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
