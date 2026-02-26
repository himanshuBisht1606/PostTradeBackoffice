using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Settlement.Batches.DTOs;
using PostTrade.Application.Features.Settlement.Batches.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Batches.Commands;

public record CreateSettlementBatchCommand(
    string SettlementNo,
    DateTime TradeDate,
    DateTime SettlementDate,
    Guid ExchangeId,
    int TotalTrades,
    decimal TotalTurnover
) : IRequest<SettlementBatchDto>;

public class CreateSettlementBatchCommandValidator : AbstractValidator<CreateSettlementBatchCommand>
{
    public CreateSettlementBatchCommandValidator()
    {
        RuleFor(x => x.SettlementNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TradeDate).NotEmpty();
        RuleFor(x => x.SettlementDate).NotEmpty().GreaterThanOrEqualTo(x => x.TradeDate);
        RuleFor(x => x.ExchangeId).NotEmpty();
        RuleFor(x => x.TotalTrades).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalTurnover).GreaterThanOrEqualTo(0);
    }
}

public class CreateSettlementBatchCommandHandler : IRequestHandler<CreateSettlementBatchCommand, SettlementBatchDto>
{
    private readonly IRepository<SettlementBatch> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateSettlementBatchCommandHandler(
        IRepository<SettlementBatch> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<SettlementBatchDto> Handle(CreateSettlementBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var batch = new SettlementBatch
        {
            BatchId = Guid.NewGuid(),
            TenantId = tenantId,
            SettlementNo = request.SettlementNo,
            TradeDate = request.TradeDate,
            SettlementDate = request.SettlementDate,
            ExchangeId = request.ExchangeId,
            TotalTrades = request.TotalTrades,
            TotalTurnover = request.TotalTurnover,
            Status = SettlementStatus.Pending
        };

        await _repo.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetSettlementBatchesQueryHandler.ToDto(batch);
    }
}
