using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.ExchangeSegments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.ExchangeSegments.Commands;

public record UpdateExchangeSegmentCommand(
    Guid ExchangeSegmentId,
    string ExchangeSegmentName,
    SettlementType SettlementType,
    bool IsActive
) : IRequest<ExchangeSegmentDto?>;

public class UpdateExchangeSegmentCommandValidator : AbstractValidator<UpdateExchangeSegmentCommand>
{
    public UpdateExchangeSegmentCommandValidator()
    {
        RuleFor(x => x.ExchangeSegmentId).NotEmpty();
        RuleFor(x => x.ExchangeSegmentName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateExchangeSegmentCommandHandler : IRequestHandler<UpdateExchangeSegmentCommand, ExchangeSegmentDto?>
{
    private readonly IRepository<ExchangeSegment> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateExchangeSegmentCommandHandler(IRepository<ExchangeSegment> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ExchangeSegmentDto?> Handle(UpdateExchangeSegmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(e => e.ExchangeSegmentId == request.ExchangeSegmentId && e.TenantId == tenantId, cancellationToken);
        var entity = results.FirstOrDefault();
        if (entity is null) return null;

        entity.ExchangeSegmentName = request.ExchangeSegmentName;
        entity.SettlementType = request.SettlementType;
        entity.IsActive = request.IsActive;

        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExchangeSegmentDto(entity.ExchangeSegmentId, entity.TenantId, entity.ExchangeId,
            entity.SegmentId, entity.ExchangeSegmentCode, entity.ExchangeSegmentName,
            entity.SettlementType, entity.IsActive);
    }
}
