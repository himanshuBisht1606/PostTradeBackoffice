using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record DeleteFoFinanceLedgerCommand(DateOnly TradeDate, string Exchange) : IRequest<int>;

public class DeleteFoFinanceLedgerCommandHandler : IRequestHandler<DeleteFoFinanceLedgerCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteFoFinanceLedgerCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<int> Handle(DeleteFoFinanceLedgerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var deleted = await _unitOfWork.ExecuteDeleteAsync<FoFinanceLedger>(
            f => f.TenantId == tenantId &&
                 f.TradeDate == request.TradeDate &&
                 f.Exchange == request.Exchange,
            cancellationToken);

        return deleted;
    }
}
