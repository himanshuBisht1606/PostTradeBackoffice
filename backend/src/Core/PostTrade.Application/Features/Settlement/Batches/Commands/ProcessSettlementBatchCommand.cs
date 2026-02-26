using MediatR;
using PostTrade.Application.Features.Settlement.Batches.DTOs;
using PostTrade.Application.Features.Settlement.Batches.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Batches.Commands;

public record ProcessSettlementBatchCommand(Guid BatchId) : IRequest<SettlementBatchDto?>;

public class ProcessSettlementBatchCommandHandler : IRequestHandler<ProcessSettlementBatchCommand, SettlementBatchDto?>
{
    private readonly IRepository<SettlementBatch> _batchRepo;
    private readonly IRepository<SettlementObligation> _obligationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ProcessSettlementBatchCommandHandler(
        IRepository<SettlementBatch> batchRepo,
        IRepository<SettlementObligation> obligationRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _batchRepo = batchRepo;
        _obligationRepo = obligationRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<SettlementBatchDto?> Handle(ProcessSettlementBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var batches = await _batchRepo.FindAsync(
            b => b.BatchId == request.BatchId && b.TenantId == tenantId, cancellationToken);
        var batch = batches.FirstOrDefault();
        if (batch is null) return null;

        if (batch.Status != SettlementStatus.Pending)
            throw new InvalidOperationException($"Batch cannot be processed in '{batch.Status}' status.");

        // Mark batch as processing
        batch.Status = SettlementStatus.Processing;
        await _batchRepo.UpdateAsync(batch, cancellationToken);

        // Settle all pending obligations in this batch
        var obligations = await _obligationRepo.FindAsync(
            o => o.BatchId == batch.BatchId && o.Status == ObligationStatus.Pending, cancellationToken);

        foreach (var obligation in obligations)
        {
            obligation.Status = ObligationStatus.Settled;
            obligation.SettledAt = DateTime.UtcNow;
            await _obligationRepo.UpdateAsync(obligation, cancellationToken);
        }

        // Mark batch as completed
        batch.Status = SettlementStatus.Completed;
        batch.ProcessedAt = DateTime.UtcNow;
        await _batchRepo.UpdateAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetSettlementBatchesQueryHandler.ToDto(batch);
    }
}
