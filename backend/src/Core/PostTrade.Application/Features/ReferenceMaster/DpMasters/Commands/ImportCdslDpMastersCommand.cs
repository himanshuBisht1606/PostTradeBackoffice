using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Common.Utilities;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.DpMasters.Commands;

public record ImportCdslDpMastersCommand(Stream CsvStream) : IRequest<ImportResultDto>;

public class ImportCdslDpMastersCommandHandler : IRequestHandler<ImportCdslDpMastersCommand, ImportResultDto>
{
    private readonly IRepository<CdslDpMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public ImportCdslDpMastersCommandHandler(IRepository<CdslDpMaster> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    // CSV has header row; uses quote-aware CsvParser
    // Key columns (0-based): 1=DpCode, 2=DpName, 3=SebiRegNo, 12=City, 13=State,
    //                         15=PinCode, 16=Phone, 19=Email, 20=MemberStatus
    public async Task<ImportResultDto> Handle(ImportCdslDpMastersCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var all = await _repo.GetAllAsync(cancellationToken);
        var existingCodes = all
            .Select(d => d.DpCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var skipped = 0;
        var errors = new List<ImportErrorDto>();
        var toInsert = new List<CdslDpMaster>();

        static string? Nullable(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        for (var i = 1; i < lines.Length; i++) // row 0 is header
        {
            var rowNumber = i + 1;
            var line = lines[i].Trim('\r').Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = CsvParser.ParseLine(line);
            if (parts.Length < 21)
            {
                errors.Add(new ImportErrorDto(rowNumber, $"Expected ≥21 columns, found {parts.Length}"));
                continue;
            }

            try
            {
                var dpCode = parts[1].Trim();

                if (string.IsNullOrEmpty(dpCode))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "DP code is empty"));
                    continue;
                }

                if (existingCodes.Contains(dpCode))
                {
                    skipped++;
                    continue;
                }

                var status = parts[20].Trim();
                if (string.IsNullOrEmpty(status)) status = "N/A";

                toInsert.Add(new CdslDpMaster
                {
                    DpId = Guid.NewGuid(),
                    DpCode = dpCode,
                    DpName = parts[2].Trim(),
                    SebiRegNo = Nullable(parts[3]),
                    City = Nullable(parts[12]),
                    State = Nullable(parts[13]),
                    PinCode = Nullable(parts[15]),
                    Phone = Nullable(parts[16]),
                    Email = Nullable(parts[19]),
                    MemberStatus = status,
                    IsActive = true,
                    CreatedBy = "import"
                });

                existingCodes.Add(dpCode);
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
