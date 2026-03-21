using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.Commands;
using PostTrade.Application.Interfaces;

namespace PostTrade.Infrastructure.FileImport;

/// <summary>
/// Watches a configured folder for new Capital Market files and dispatches import commands.
/// Triggered on file creation via FileSystemWatcher.
/// </summary>
public class FileWatcherService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly string _watchFolder;
    private readonly Guid _tenantId;
    private FileSystemWatcher? _watcher;

    public FileWatcherService(
        IServiceScopeFactory scopeFactory,
        ILogger<FileWatcherService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _watchFolder = configuration["FileImport:WatchFolder"] ?? string.Empty;
        Guid.TryParse(configuration["FileImport:TenantId"], out _tenantId);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_watchFolder) || !Directory.Exists(_watchFolder))
        {
            _logger.LogWarning("FileWatcherService: WatchFolder '{Folder}' is not configured or does not exist. Service inactive.", _watchFolder);
            return Task.CompletedTask;
        }

        _watcher = new FileSystemWatcher(_watchFolder, "*.csv")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileCreated;
        _logger.LogInformation("FileWatcherService started. Watching: {Folder}", _watchFolder);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        _logger.LogInformation("FileWatcherService stopped.");
        return Task.CompletedTask;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await DispatchFileAsync(e.FullPath, e.Name ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FileWatcherService: Error processing file {File}", e.FullPath);
            }
        });
    }

    private async Task DispatchFileAsync(string fullPath, string fileName)
    {
        if (!TryDetectFileInfo(fileName, out var fileType, out var exchange, out var tradingDate))
        {
            _logger.LogWarning("FileWatcherService: Could not detect file type from filename '{File}'. Skipping.", fileName);
            return;
        }

        // Brief delay to ensure file write is complete
        await Task.Delay(TimeSpan.FromSeconds(2));

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        if (_tenantId == Guid.Empty)
        {
            _logger.LogWarning("FileWatcherService: TenantId not configured. Cannot dispatch import for {File}.", fileName);
            return;
        }

        tenantContext.SetTenantId(_tenantId);

        await using var stream = File.OpenRead(fullPath);
        await DispatchCommand(mediator, fileType, stream, tradingDate, exchange, "FolderWatch", fileName, CancellationToken.None);

        _logger.LogInformation("FileWatcherService: Dispatched import for {File}", fileName);
    }

    internal static bool TryDetectFileInfo(string fileName, out string fileType, out string exchange, out DateOnly tradingDate)
    {
        fileType = string.Empty;
        exchange = string.Empty;
        tradingDate = default;

        // Expected pattern: {FileType}_{Exchange}_CM_{YYYYMMDD}_{...}.csv
        // e.g. Trade_NSE_CM_20251028_...csv
        var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
        if (parts.Length < 4) return false;

        fileType = parts[0].ToUpperInvariant();
        exchange = parts[1].ToUpperInvariant();

        // Date is at position index 3 (0-based)
        if (!DateOnly.TryParseExact(parts[3], "yyyyMMdd", out tradingDate))
            return false;

        // Normalize exchange for NCL files (Margin, Obligation, STT, StampDuty)
        if (exchange == "NCL") exchange = "NSE";

        return fileType is "TRADE" or "BHAVCOPY" or "MARGIN" or "OBLIGATION" or "STT" or "STAMPDUTY";
    }

    private static async Task DispatchCommand(
        IMediator mediator,
        string fileType,
        Stream stream,
        DateOnly tradingDate,
        string exchange,
        string triggerSource,
        string fileName,
        CancellationToken ct)
    {
        switch (fileType)
        {
            case "TRADE":
                await mediator.Send(new ImportCmTradeFileCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "BHAVCOPY":
                await mediator.Send(new ImportCmBhavCopyCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "MARGIN":
                await mediator.Send(new ImportCmMarginCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "OBLIGATION":
                await mediator.Send(new ImportCmObligationCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "STT":
                await mediator.Send(new ImportCmSttCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "STAMPDUTY":
                await mediator.Send(new ImportCmStampDutyCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
