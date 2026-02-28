using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.PinCodes.Commands;

public record ImportPinCodeMastersCommand(Stream CsvStream) : IRequest<ImportResultDto>;

public class ImportPinCodeMastersCommandHandler : IRequestHandler<ImportPinCodeMastersCommand, ImportResultDto>
{
    private readonly IRepository<PinCodeMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public ImportPinCodeMastersCommandHandler(IRepository<PinCodeMaster> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    // CSV has header row; columns: pincode,district,city,state_id,country_id,mcx_code
    public async Task<ImportResultDto> Handle(ImportPinCodeMastersCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var all = await _repo.GetAllAsync(cancellationToken);
        var existingCodes = all
            .Select(p => p.PinCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var skipped = 0;
        var errors = new List<ImportErrorDto>();
        var toInsert = new List<PinCodeMaster>();

        static string? Nullable(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        for (var i = 1; i < lines.Length; i++) // row 0 is header
        {
            var rowNumber = i + 1;
            var line = lines[i].Trim('\r').Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 6)
            {
                errors.Add(new ImportErrorDto(rowNumber, $"Expected 6 columns, found {parts.Length}"));
                continue;
            }

            try
            {
                var pinCode = parts[0].Trim();

                if (string.IsNullOrEmpty(pinCode))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "Pin code is empty"));
                    continue;
                }

                if (existingCodes.Contains(pinCode))
                {
                    skipped++;
                    continue;
                }

                toInsert.Add(new PinCodeMaster
                {
                    PinCodeId = Guid.NewGuid(),
                    PinCode = pinCode,
                    District = Nullable(parts[1]),
                    City = Nullable(parts[2]),
                    StateCode = parts[3].Trim(),
                    CountryCode = parts[4].Trim(),
                    McxCode = Nullable(parts[5]),
                    IsActive = true,
                    CreatedBy = "import"
                });

                existingCodes.Add(pinCode);
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNumber, ex.Message));
            }
        }

        // Batch insert — 2000 rows per SaveChanges; clear tracker between batches
        foreach (var batch in toInsert.Chunk(2000))
        {
            await _repo.AddRangeAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _unitOfWork.ClearTracking();
        }

        return new ImportResultDto(toInsert.Count, skipped, errors);
    }
}
