using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Segments.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Segments.Commands;

public record UpdateSegmentCommand(Guid SegmentId, string SegmentName, bool IsActive) : IRequest<SegmentDto?>;

public class UpdateSegmentCommandValidator : AbstractValidator<UpdateSegmentCommand>
{
    public UpdateSegmentCommandValidator()
    {
        RuleFor(x => x.SegmentId).NotEmpty();
        RuleFor(x => x.SegmentName).NotEmpty().MaximumLength(100);
    }
}

public class UpdateSegmentCommandHandler : IRequestHandler<UpdateSegmentCommand, SegmentDto?>
{
    private readonly IRepository<Segment> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateSegmentCommandHandler(IRepository<Segment> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<SegmentDto?> Handle(UpdateSegmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(s => s.SegmentId == request.SegmentId && s.TenantId == tenantId, cancellationToken);
        var segment = results.FirstOrDefault();
        if (segment is null) return null;

        segment.SegmentName = request.SegmentName;
        segment.IsActive = request.IsActive;

        await _repo.UpdateAsync(segment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SegmentDto(segment.SegmentId, segment.TenantId, segment.ExchangeId,
            segment.SegmentCode, segment.SegmentName, segment.IsActive);
    }
}
