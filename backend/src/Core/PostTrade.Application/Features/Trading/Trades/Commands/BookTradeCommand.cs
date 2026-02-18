using FluentValidation;
using MediatR;
using PostTrade.Application.Features.Trading.Trades.DTOs;
using PostTrade.Application.Features.Trading.Trades.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Trading.Trades.Commands;

public record BookTradeCommand(
    Guid BrokerId,
    Guid ClientId,
    Guid InstrumentId,
    TradeSide Side,
    int Quantity,
    decimal Price,
    DateTime TradeDate,
    string SettlementNo,
    TradeSource Source,
    string? ExchangeTradeNo,
    string? SourceReference
) : IRequest<TradeDto>;

public class BookTradeCommandValidator : AbstractValidator<BookTradeCommand>
{
    public BookTradeCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.InstrumentId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.TradeDate).NotEmpty();
        RuleFor(x => x.SettlementNo).NotEmpty().MaximumLength(50);
    }
}

public class BookTradeCommandHandler : IRequestHandler<BookTradeCommand, TradeDto>
{
    private readonly IRepository<Trade> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public BookTradeCommandHandler(IRepository<Trade> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<TradeDto> Handle(BookTradeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var tradeValue = request.Quantity * request.Price;

        var trade = new Trade
        {
            TradeId = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerId = request.BrokerId,
            ClientId = request.ClientId,
            InstrumentId = request.InstrumentId,
            TradeNo = GenerateTradeNo(),
            ExchangeTradeNo = request.ExchangeTradeNo,
            Side = request.Side,
            Quantity = request.Quantity,
            Price = request.Price,
            TradeValue = tradeValue,
            TradeDate = request.TradeDate.Date,
            TradeTime = DateTime.UtcNow,
            SettlementNo = request.SettlementNo,
            Source = request.Source,
            SourceReference = request.SourceReference,
            Status = TradeStatus.Pending,
            NetAmount = tradeValue  // charges calculated in Ledger phase
        };

        await _repo.AddAsync(trade, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetTradesQueryHandler.ToDto(trade);
    }

    private static string GenerateTradeNo() =>
        $"TRD{DateTime.UtcNow:yyyyMMddHHmmssfff}";
}
