using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;

public record DeleteCmImportBatchCommand(Guid BatchId) : IRequest<bool>;

public class DeleteCmImportBatchCommandHandler : IRequestHandler<DeleteCmImportBatchCommand, bool>
{
    private readonly IRepository<CmFileImportBatch> _batchRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteCmImportBatchCommandHandler(
        IRepository<CmFileImportBatch> batchRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(DeleteCmImportBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var batch = await _batchRepo.FirstOrDefaultAsync(
            b => b.BatchId == request.BatchId && b.TenantId == tenantId,
            cancellationToken);

        if (batch == null) return false;

        // Hard-delete all linked data rows via bulk delete
        await _unitOfWork.ExecuteDeleteAsync<CmTrade>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<CmBhavCopy>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<CmMargin>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<CmObligation>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<CmStt>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<CmStampDuty>(t => t.BatchId == request.BatchId, cancellationToken);
        await _unitOfWork.ExecuteDeleteAsync<CmFileImportLog>(t => t.BatchId == request.BatchId, cancellationToken);

        await _batchRepo.DeleteAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
