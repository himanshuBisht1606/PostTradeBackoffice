using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.States.Commands;

public record ImportStatesCommand(Stream CsvStream) : IRequest<ImportResultDto>;

public class ImportStatesCommandHandler : IRequestHandler<ImportStatesCommand, ImportResultDto>
{
    private readonly IRepository<StateMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public ImportStatesCommandHandler(IRepository<StateMaster> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    // CSV columns: COUNTRY_ID,STATE_ID,STATE_NAME,NSE,BSE,CVL,NDML,NCDEX,NSEKRA,NSDL
    public async Task<ImportResultDto> Handle(ImportStatesCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var all = await _repo.GetAllAsync(cancellationToken);
        var existingCodes = all
            .Select(s => s.StateCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var skipped = 0;
        var errors = new List<ImportErrorDto>();
        var toInsert = new List<StateMaster>();

        static int? ParseInt(string s)
        {
            s = s.Trim();
            return string.IsNullOrEmpty(s) ? null : int.TryParse(s, out var v) ? v : (int?)null;
        }

        for (var i = 1; i < lines.Length; i++) // row 0 is header
        {
            var rowNumber = i + 1;
            var line = lines[i].Trim('\r').Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 10)
            {
                errors.Add(new ImportErrorDto(rowNumber, $"Expected 10 columns, found {parts.Length}"));
                continue;
            }

            try
            {
                var countryId = parts[0].Trim();
                var stateCode = parts[1].Trim();
                var stateName = parts[2].Trim();

                if (string.IsNullOrEmpty(stateCode))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "State code is empty"));
                    continue;
                }

                if (existingCodes.Contains(stateCode))
                {
                    skipped++;
                    continue;
                }

                var bseName = parts[4].Trim();
                var state = new StateMaster
                {
                    StateId = Guid.NewGuid(),
                    CountryId = string.IsNullOrEmpty(countryId) ? "IN" : countryId,
                    StateCode = stateCode,
                    StateName = stateName,
                    NseCode = ParseInt(parts[3]),
                    BseName = string.IsNullOrEmpty(bseName) ? null : bseName,
                    CvlCode = ParseInt(parts[5]),
                    NdmlCode = ParseInt(parts[6]),
                    NcdexCode = ParseInt(parts[7]),
                    NseKraCode = ParseInt(parts[8]),
                    NsdlCode = ParseInt(parts[9]),
                    IsActive = true,
                    CreatedBy = "import"
                };

                toInsert.Add(state);
                existingCodes.Add(stateCode);
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNumber, ex.Message));
            }
        }

        foreach (var state in toInsert)
            await _repo.AddAsync(state, cancellationToken);

        if (toInsert.Count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResultDto(toInsert.Count, skipped, errors);
    }
}
