using MediatR;
using PostTrade.Application.Common.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.PostTrade.FO.Commands;

public record ImportFoContractMasterCommand(
    Stream CsvStream,
    string ExchangeCode,
    DateOnly TradingDate
) : IRequest<ImportResultDto>;

public class ImportFoContractMasterCommandHandler : IRequestHandler<ImportFoContractMasterCommand, ImportResultDto>
{
    private readonly IRepository<Instrument> _instrumentRepo;
    private readonly IRepository<Exchange> _exchangeRepo;
    private readonly IRepository<ExchangeSegment> _exchangeSegmentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ImportFoContractMasterCommandHandler(
        IRepository<Instrument> instrumentRepo,
        IRepository<Exchange> exchangeRepo,
        IRepository<ExchangeSegment> exchangeSegmentRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _instrumentRepo = instrumentRepo;
        _exchangeRepo = exchangeRepo;
        _exchangeSegmentRepo = exchangeSegmentRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ImportResultDto> Handle(ImportFoContractMasterCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        var exchanges = await _exchangeRepo.FindAsync(
            e => e.TenantId == tenantId && e.ExchangeCode.ToUpper() == request.ExchangeCode.ToUpper(),
            cancellationToken);
        var exchange = exchanges.FirstOrDefault();
        if (exchange is null)
            return new ImportResultDto(0, 0, [new ImportErrorDto(0, $"Exchange '{request.ExchangeCode}' not found for this tenant")]);

        var segments = await _exchangeSegmentRepo.FindAsync(
            es => es.ExchangeId == exchange.ExchangeId && es.IsActive,
            cancellationToken);
        var exchangeSegment = segments.FirstOrDefault();
        if (exchangeSegment is null)
            return new ImportResultDto(0, 0, [new ImportErrorDto(0, $"No active segment found for exchange '{request.ExchangeCode}'")]);

        // Change (b): projection-only duplicate check — avoids loading full Instrument entities
        var existingCodes = (await _instrumentRepo.FindProjectedAsync(
            i => i.TenantId == tenantId && i.ExchangeId == exchange.ExchangeId,
            i => i.InstrumentCode,
            cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Change (a): stream line-by-line instead of reading the entire file into memory
        using var reader = new StreamReader(request.CsvStream, leaveOpen: true);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine is null)
            return new ImportResultDto(0, 0, []);

        var header = headerLine.Trim('\r').Split(',');
        var colIndex = BuildColumnIndex(header);

        var skipped = 0;
        var errors = new List<ImportErrorDto>();

        // Change (c): batch inserts with tracker clear every 500 rows
        const int batchSize = 500;
        var batch = new List<Instrument>(batchSize);
        var totalCreated = 0;
        var rowNumber = 1; // header was row 1; data rows start at 2

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            rowNumber++;
            var trimmed = line.Trim('\r').Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            var parts = SplitCsvLine(trimmed);

            try
            {
                if (!TryGetValue(parts, colIndex, "INSTRUMENT", out var instrumentTypeStr))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "Missing INSTRUMENT column"));
                    continue;
                }

                if (!TryGetValue(parts, colIndex, "SYMBOL", out var symbol) || string.IsNullOrWhiteSpace(symbol))
                {
                    errors.Add(new ImportErrorDto(rowNumber, "Missing or empty SYMBOL"));
                    continue;
                }

                TryGetValue(parts, colIndex, "EXPIRY_DT", out var expiryStr);
                TryGetValue(parts, colIndex, "STRIKE_PR", out var strikePrStr);
                TryGetValue(parts, colIndex, "OPTION_TYP", out var optionTypeStr);
                TryGetValue(parts, colIndex, "LOT_SIZE", out var lotSizeStr);
                TryGetValue(parts, colIndex, "TICK_SIZE", out var tickSizeStr);
                TryGetValue(parts, colIndex, "UNDERLYING", out var underlying);

                var instrumentType = MapInstrumentType(instrumentTypeStr);
                var expiryDate = ParseExpiryDate(expiryStr);
                var strikePrice = decimal.TryParse(strikePrStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var sp) ? sp : 0m;
                var optionType = MapOptionType(optionTypeStr);
                var lotSize = decimal.TryParse(lotSizeStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var ls) && ls > 0 ? ls : 1m;
                var tickSize = decimal.TryParse(tickSizeStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var ts) && ts > 0 ? ts : 0.05m;

                var instrumentCode = BuildCode(symbol, instrumentType, expiryDate, strikePrice, optionType);

                if (existingCodes.Contains(instrumentCode))
                {
                    skipped++;
                    continue;
                }

                var instrument = new Instrument
                {
                    InstrumentId = Guid.NewGuid(),
                    TenantId = tenantId,
                    InstrumentCode = instrumentCode,
                    InstrumentName = string.IsNullOrWhiteSpace(underlying) ? symbol : underlying,
                    Symbol = symbol,
                    ExchangeId = exchange.ExchangeId,
                    SegmentId = exchangeSegment.SegmentId,
                    InstrumentType = instrumentType,
                    LotSize = lotSize,
                    TickSize = tickSize,
                    ExpiryDate = expiryDate,
                    StrikePrice = strikePrice > 0 ? strikePrice : null,
                    OptionType = optionType,
                    Status = InstrumentStatus.Active,
                    CreatedBy = "import"
                };

                batch.Add(instrument);
                existingCodes.Add(instrumentCode);

                if (batch.Count >= batchSize)
                {
                    await _instrumentRepo.AddRangeAsync(batch, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _unitOfWork.ClearTracking();
                    totalCreated += batch.Count;
                    batch.Clear();
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(rowNumber, ex.Message));
            }
        }

        // Flush remaining rows that did not fill a complete batch
        if (batch.Count > 0)
        {
            await _instrumentRepo.AddRangeAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            totalCreated += batch.Count;
        }

        return new ImportResultDto(totalCreated, skipped, errors);
    }

    private static Dictionary<string, int> BuildColumnIndex(string[] header)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["INSTRUMENT_TYPE"] = "INSTRUMENT",
            ["INST_TYPE"] = "INSTRUMENT",
            ["EXPIRY"] = "EXPIRY_DT",
            ["EXPIRY_DATE"] = "EXPIRY_DT",
            ["EXPDT"] = "EXPIRY_DT",
            ["STRIKE_PRICE"] = "STRIKE_PR",
            ["STRIKEPRICE"] = "STRIKE_PR",
            ["OPTION_TYPE"] = "OPTION_TYP",
            ["OPTIONTYPECODE"] = "OPTION_TYP",
            ["UNIT_OF_TRADING"] = "LOT_SIZE",
            ["MKTLOT"] = "LOT_SIZE",
            ["TICKSIZE"] = "TICK_SIZE",
            ["UNDERLYING_SYMBOL"] = "UNDERLYING",
            ["SYM"] = "SYMBOL",
        };

        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Length; i++)
        {
            var col = header[i].Trim().Trim('"').ToUpper();
            index[col] = i;
            if (aliases.TryGetValue(col, out var canonical))
                index.TryAdd(canonical, i);
        }
        return index;
    }

    private static bool TryGetValue(string[] parts, Dictionary<string, int> colIndex, string key, out string value)
    {
        if (colIndex.TryGetValue(key, out var idx) && idx < parts.Length)
        {
            value = parts[idx].Trim().Trim('"');
            return true;
        }
        value = string.Empty;
        return false;
    }

    private static string[] SplitCsvLine(string line)
    {
        var parts = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();
        foreach (var ch in line)
        {
            if (ch == '"') { inQuotes = !inQuotes; continue; }
            if (ch == ',' && !inQuotes) { parts.Add(current.ToString()); current.Clear(); continue; }
            current.Append(ch);
        }
        parts.Add(current.ToString());
        return [.. parts];
    }

    private static InstrumentType MapInstrumentType(string? raw) => raw?.Trim().ToUpper() switch
    {
        "FUTIDX" or "FUTIVX" or "FUTSTK" => InstrumentType.Future,
        "FUTCUR" => InstrumentType.Currency,
        "OPTIDX" or "OPTSTK" or "OPTFUT" => InstrumentType.Option,
        _ => InstrumentType.Derivative
    };

    private static OptionType? MapOptionType(string? raw) => raw?.Trim().ToUpper() switch
    {
        "CE" or "CALL" or "C" => OptionType.Call,
        "PE" or "PUT" or "P" => OptionType.Put,
        _ => null
    };

    private static DateTime? ParseExpiryDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();

        string[] formats = ["d-MMM-yyyy", "ddMMyyyy", "dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "d-MMM-yy", "ddMMMyyyy"];
        if (DateTime.TryParseExact(raw, formats, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dt))
            return dt;

        if (DateTime.TryParse(raw, out dt)) return dt;
        return null;
    }

    private static string BuildCode(string symbol, InstrumentType type, DateTime? expiry, decimal strike, OptionType? optionType)
    {
        var expiryPart = expiry.HasValue ? expiry.Value.ToString("yyyyMMdd") : "NOEXP";
        return type switch
        {
            InstrumentType.Option => $"{symbol}_{expiryPart}_{strike:F0}_{(optionType == OptionType.Call ? "CE" : "PE")}",
            InstrumentType.Future or InstrumentType.Currency => $"{symbol}_{expiryPart}_FUT",
            _ => $"{symbol}_{type}"
        };
    }
}
