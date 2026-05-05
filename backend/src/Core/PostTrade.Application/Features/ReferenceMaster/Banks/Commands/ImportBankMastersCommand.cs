using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.ReferenceMaster.Banks.Commands;

public record ImportBankMastersCommand(Stream CsvStream) : IRequest<ImportResultDto>;

public class ImportBankMastersCommandHandler : IRequestHandler<ImportBankMastersCommand, ImportResultDto>
{
    private readonly IRepository<BankMaster> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public ImportBankMastersCommandHandler(IRepository<BankMaster> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    // CSV columns (no header, 0-based): 0=BankCode, 1=BankName, 2=IFSCPrefix
    public async Task<ImportResultDto> Handle(ImportBankMastersCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var all = await _repo.GetAllAsync(cancellationToken);
        var existingCodes = all
            .Select(b => b.BankCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var skipped = 0;
        var errors = new List<ImportErrorDto>();
        var toInsert = new List<BankMaster>();

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
                var bankName = parts[1].Trim();
                var ifscPrefix = parts[2].Trim();

                if (string.IsNullOrEmpty(bankCode))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "Bank code is empty"));
                    continue;
                }

                if (existingCodes.Contains(bankCode))
                {
                    skipped++;
                    continue;
                }

                toInsert.Add(new BankMaster
                {
                    BankId = Guid.NewGuid(),
                    BankCode = bankCode,
                    BankName = bankName,
                    IFSCPrefix = ifscPrefix,
                    IsActive = true,
                    CreatedBy = "import"
                });

                existingCodes.Add(bankCode);
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNumber, ex.Message));
            }
        }

        if (toInsert.Count > 0)
        {
            await _repo.AddRangeAsync(toInsert, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ImportResultDto(toInsert.Count, skipped, errors);
    }
}
