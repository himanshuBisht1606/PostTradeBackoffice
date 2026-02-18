using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Exchanges.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Exchanges.Commands;

public record CreateExchangeCommand(
    string ExchangeCode,
    string ExchangeName,
    string Country,
    string? TimeZone,
    TimeOnly? TradingStartTime,
    TimeOnly? TradingEndTime
) : IRequest<ExchangeDto>;

public class CreateExchangeCommandValidator : AbstractValidator<CreateExchangeCommand>
{
    public CreateExchangeCommandValidator()
    {
        RuleFor(x => x.ExchangeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ExchangeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class CreateExchangeCommandHandler : IRequestHandler<CreateExchangeCommand, ExchangeDto>
{
    private readonly IRepository<Exchange> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateExchangeCommandHandler(IRepository<Exchange> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ExchangeDto> Handle(CreateExchangeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var exchange = new Exchange
        {
            ExchangeId = Guid.NewGuid(),
            TenantId = tenantId,
            ExchangeCode = request.ExchangeCode,
            ExchangeName = request.ExchangeName,
            Country = request.Country,
            TimeZone = request.TimeZone,
            TradingStartTime = request.TradingStartTime,
            TradingEndTime = request.TradingEndTime,
            IsActive = true
        };

        await _repo.AddAsync(exchange, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExchangeDto(exchange.ExchangeId, exchange.TenantId, exchange.ExchangeCode,
            exchange.ExchangeName, exchange.Country, exchange.TimeZone,
            exchange.TradingStartTime, exchange.TradingEndTime, exchange.IsActive);
    }
}
