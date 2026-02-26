using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Exchanges.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Exchanges.Commands;

public record UpdateExchangeCommand(
    Guid ExchangeId,
    string ExchangeName,
    string Country,
    string? TimeZone,
    TimeOnly? TradingStartTime,
    TimeOnly? TradingEndTime,
    bool IsActive
) : IRequest<ExchangeDto?>;

public class UpdateExchangeCommandValidator : AbstractValidator<UpdateExchangeCommand>
{
    public UpdateExchangeCommandValidator()
    {
        RuleFor(x => x.ExchangeId).NotEmpty();
        RuleFor(x => x.ExchangeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class UpdateExchangeCommandHandler : IRequestHandler<UpdateExchangeCommand, ExchangeDto?>
{
    private readonly IRepository<Exchange> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateExchangeCommandHandler(IRepository<Exchange> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ExchangeDto?> Handle(UpdateExchangeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(e => e.ExchangeId == request.ExchangeId && e.TenantId == tenantId, cancellationToken);
        var exchange = results.FirstOrDefault();
        if (exchange is null) return null;

        exchange.ExchangeName = request.ExchangeName;
        exchange.Country = request.Country;
        exchange.TimeZone = request.TimeZone;
        exchange.TradingStartTime = request.TradingStartTime;
        exchange.TradingEndTime = request.TradingEndTime;
        exchange.IsActive = request.IsActive;

        await _repo.UpdateAsync(exchange, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExchangeDto(exchange.ExchangeId, exchange.TenantId, exchange.ExchangeCode,
            exchange.ExchangeName, exchange.Country, exchange.TimeZone,
            exchange.TradingStartTime, exchange.TradingEndTime, exchange.IsActive);
    }
}
