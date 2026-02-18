using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Segments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Segments.Commands;

public record CreateSegmentCommand(
    Guid ExchangeId,
    string SegmentCode,
    string SegmentName
) : IRequest<SegmentDto>;

public class CreateSegmentCommandValidator : AbstractValidator<CreateSegmentCommand>
{
    public CreateSegmentCommandValidator()
    {
        RuleFor(x => x.ExchangeId).NotEmpty();
        RuleFor(x => x.SegmentCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SegmentName).NotEmpty().MaximumLength(100);
    }
}

public class CreateSegmentCommandHandler : IRequestHandler<CreateSegmentCommand, SegmentDto>
{
    private readonly IRepository<Segment> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateSegmentCommandHandler(IRepository<Segment> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<SegmentDto> Handle(CreateSegmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            TenantId = tenantId,
            ExchangeId = request.ExchangeId,
            SegmentCode = request.SegmentCode,
            SegmentName = request.SegmentName,
            IsActive = true
        };

        await _repo.AddAsync(segment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SegmentDto(segment.SegmentId, segment.TenantId, segment.ExchangeId,
            segment.SegmentCode, segment.SegmentName, segment.IsActive);
    }
}
