using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.ExchangeSegments.Commands;

public record CreateExchangeSegmentCommand(
    Guid ExchangeId,
    Guid SegmentId,
    string ExchangeSegmentCode,
    string ExchangeSegmentName,
    SettlementType SettlementType
) : IRequest<ExchangeSegmentDto>;

public class CreateExchangeSegmentCommandValidator : AbstractValidator<CreateExchangeSegmentCommand>
{
    public CreateExchangeSegmentCommandValidator()
    {
        RuleFor(x => x.ExchangeId).NotEmpty();
        RuleFor(x => x.SegmentId).NotEmpty();
        RuleFor(x => x.ExchangeSegmentCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ExchangeSegmentName).NotEmpty().MaximumLength(200);
    }
}

public class CreateExchangeSegmentCommandHandler : IRequestHandler<CreateExchangeSegmentCommand, ExchangeSegmentDto>
{
    private readonly IRepository<ExchangeSegment> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateExchangeSegmentCommandHandler(IRepository<ExchangeSegment> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ExchangeSegmentDto> Handle(CreateExchangeSegmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var entity = new ExchangeSegment
        {
            ExchangeSegmentId = Guid.NewGuid(),
            TenantId = tenantId,
            ExchangeId = request.ExchangeId,
            SegmentId = request.SegmentId,
            ExchangeSegmentCode = request.ExchangeSegmentCode,
            ExchangeSegmentName = request.ExchangeSegmentName,
            SettlementType = request.SettlementType,
            IsActive = true
        };

        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExchangeSegmentDto(entity.ExchangeSegmentId, entity.TenantId, entity.ExchangeId,
            entity.SegmentId, entity.ExchangeSegmentCode, entity.ExchangeSegmentName,
            entity.SettlementType, entity.IsActive);
    }
}
