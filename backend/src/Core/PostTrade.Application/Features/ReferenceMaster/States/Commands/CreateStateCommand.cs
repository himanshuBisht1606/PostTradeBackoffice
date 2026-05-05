using FluentValidation;
using FluentValidation.Results;
using MediatR;
using PostTrade.Application.Features.ReferenceMaster.States.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.States.Commands;

public record CreateStateCommand(
    string CountryId,
    string StateCode,
    string StateName,
    int? NseCode,
    string? BseName,
    int? CvlCode,
    int? NdmlCode,
    int? NcdexCode,
    int? NseKraCode,
    int? NsdlCode
) : IRequest<StateMasterDto>;

public class CreateStateCommandValidator : AbstractValidator<CreateStateCommand>
{
    public CreateStateCommandValidator()
    {
        RuleFor(x => x.StateCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.StateName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CountryId).NotEmpty().MaximumLength(5);
    }
}

public class CreateStateCommandHandler : IRequestHandler<CreateStateCommand, StateMasterDto>
{
    private readonly IRepository<StateMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStateCommandHandler(IRepository<StateMaster> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<StateMasterDto> Handle(CreateStateCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.FindAsync(s => s.StateCode == request.StateCode, cancellationToken);
        if (existing.Any())
            throw new ValidationException(new[] { new ValidationFailure("StateCode", "State code already exists") });

        var state = new StateMaster
        {
            StateId = Guid.NewGuid(),
            CountryId = request.CountryId,
            StateCode = request.StateCode,
            StateName = request.StateName,
            NseCode = request.NseCode,
            BseName = request.BseName,
            CvlCode = request.CvlCode,
            NdmlCode = request.NdmlCode,
            NcdexCode = request.NcdexCode,
            NseKraCode = request.NseKraCode,
            NsdlCode = request.NsdlCode,
            IsActive = true,
            CreatedBy = "system"
        };

        await _repo.AddAsync(state, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StateMasterDto(
            state.StateId, state.CountryId, state.StateCode, state.StateName,
            state.NseCode, state.BseName, state.CvlCode, state.NdmlCode,
            state.NcdexCode, state.NseKraCode, state.NsdlCode, state.IsActive);
    }
}
