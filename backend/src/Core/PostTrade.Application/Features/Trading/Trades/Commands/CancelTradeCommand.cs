using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Trading.Trades.DTOs;
using PostTrade.Application.Features.Trading.Trades.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Trading.Trades.Commands;

public record CancelTradeCommand(Guid TradeId, string Reason) : IRequest<TradeDto?>;

public class CancelTradeCommandValidator : AbstractValidator<CancelTradeCommand>
{
    public CancelTradeCommandValidator()
    {
        RuleFor(x => x.TradeId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CancelTradeCommandHandler : IRequestHandler<CancelTradeCommand, TradeDto?>
{
    private readonly IRepository<Trade> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CancelTradeCommandHandler(IRepository<Trade> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<TradeDto?> Handle(CancelTradeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            t => t.TradeId == request.TradeId && t.TenantId == tenantId, cancellationToken);
        var trade = results.FirstOrDefault();
        if (trade is null) return null;

        if (trade.Status is not (TradeStatus.Pending or TradeStatus.Validated))
            throw new InvalidOperationException($"Trade cannot be cancelled in '{trade.Status}' status.");

        trade.Status = TradeStatus.Cancelled;
        trade.RejectionReason = request.Reason;

        await _repo.UpdateAsync(trade, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetTradesQueryHandler.ToDto(trade);
    }
}
