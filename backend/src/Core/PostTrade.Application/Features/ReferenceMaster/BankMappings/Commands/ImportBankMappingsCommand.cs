using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.BankMappings.Commands;

public record ImportBankMappingsCommand(Stream CsvStream) : IRequest<ImportResultDto>;

public class ImportBankMappingsCommandHandler : IRequestHandler<ImportBankMappingsCommand, ImportResultDto>
{
    private readonly IRepository<BankMapping> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public ImportBankMappingsCommandHandler(IRepository<BankMapping> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    // CSV columns (no header, 0-based): 0=BankCode, 1=IFSCCode, 2=MICRCode
    public async Task<ImportResultDto> Handle(ImportBankMappingsCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var all = await _repo.GetAllAsync(cancellationToken);
        var existingIfsc = all
            .Select(m => m.IFSCCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var skipped = 0;
        var errors = new List<ImportErrorDto>();
        var toInsert = new List<BankMapping>();

        for (var i = 0; i < lines.Length; i++)
        {
            var rowNumber = i + 1;
            var line = lines[i].Trim('\r').Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 3)
            {
                errors.Add(new ImportErrorDto(rowNumber, $"Expected 3 columns, found {parts.Length}"));
                continue;
            }

            try
            {
                var bankCode = parts[0].Trim();
                var ifscCode = parts[1].Trim();
                var micrCode = parts[2].Trim();

                if (string.IsNullOrEmpty(ifscCode))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "IFSC code is empty"));
                    continue;
                }

                if (existingIfsc.Contains(ifscCode))
                {
                    skipped++;
                    continue;
                }

                toInsert.Add(new BankMapping
                {
                    MappingId = Guid.NewGuid(),
                    BankCode = bankCode,
                    IFSCCode = ifscCode,
                    MICRCode = micrCode,
                    IsActive = true,
                    CreatedBy = "import"
                });

                existingIfsc.Add(ifscCode);
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNumber, ex.Message));
            }
        }

        // Batch insert — 2000 rows per SaveChanges; clear tracker to prevent quadratic overhead
        foreach (var batch in toInsert.Chunk(2000))
        {
            await _repo.AddRangeAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _unitOfWork.ClearTracking();
        }

        return new ImportResultDto(toInsert.Count, skipped, errors);
    }
}
