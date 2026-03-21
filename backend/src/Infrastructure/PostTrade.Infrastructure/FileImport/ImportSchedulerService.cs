using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PostTrade.Application.Interfaces;

namespace PostTrade.Infrastructure.FileImport;

/// <summary>
/// Scans the configured folder once per day at a configured time and imports any files for today's date.
/// Uses PeriodicTimer to fire at the configured schedule time.
/// </summary>
public class ImportSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImportSchedulerService> _logger;
    private readonly string _watchFolder;
    private readonly TimeOnly _scheduleTime;
    private readonly Guid _tenantId;

    public ImportSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<ImportSchedulerService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _watchFolder = configuration["FileImport:WatchFolder"] ?? string.Empty;
        Guid.TryParse(configuration["FileImport:TenantId"], out _tenantId);

        var scheduleStr = configuration["FileImport:ScheduleTime"] ?? "19:00";
        _scheduleTime = TimeOnly.TryParse(scheduleStr, out var t) ? t : new TimeOnly(19, 0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_watchFolder))
        {
            _logger.LogWarning("ImportSchedulerService: WatchFolder not configured. Service inactive.");
            return;
        }

        _logger.LogInformation("ImportSchedulerService started. Schedule: {Time} daily. Watching: {Folder}", _scheduleTime, _watchFolder);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var nextRun = DateTime.Today.Add(_scheduleTime.ToTimeSpan());
            if (now >= _scheduleTime)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - DateTime.Now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            _logger.LogInformation("ImportSchedulerService: Next run at {NextRun}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunScanAsync(DateOnly.FromDateTime(DateTime.Today), stoppingToken);
        }
    }

    private async Task RunScanAsync(DateOnly tradingDate, CancellationToken ct)
    {
        _logger.LogInformation("ImportSchedulerService: Running scan for {Date}", tradingDate);

        if (!Directory.Exists(_watchFolder))
        {
            _logger.LogWarning("ImportSchedulerService: WatchFolder '{Folder}' does not exist.", _watchFolder);
            return;
        }

        var files = Directory.GetFiles(_watchFolder, "*.csv");
        var dateStr = tradingDate.ToString("yyyyMMdd");

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            if (!fileName.Contains(dateStr)) continue;

            if (!FileWatcherService.TryDetectFileInfo(fileName, out var fileType, out var exchange, out _))
            {
                _logger.LogDebug("ImportSchedulerService: Skipping unrecognised file {File}", fileName);
                continue;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

                if (_tenantId == Guid.Empty)
                {
                    _logger.LogWarning("ImportSchedulerService: TenantId not configured. Skipping {File}.", fileName);
                    continue;
                }

                tenantContext.SetTenantId(_tenantId);

                await using var stream = File.OpenRead(filePath);
                await DispatchFile(mediator, fileType, stream, tradingDate, exchange, "Scheduler", fileName, ct);

                _logger.LogInformation("ImportSchedulerService: Imported {File}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportSchedulerService: Failed to import {File}", fileName);
            }
        }
    }

    private static async Task DispatchFile(
        IMediator mediator,
        string fileType,
        Stream stream,
        DateOnly tradingDate,
        string exchange,
        string triggerSource,
        string fileName,
        CancellationToken ct)
    {
        // Reuse the same dispatch logic pattern as FileWatcherService
        switch (fileType)
        {
            case "TRADE":
                await mediator.Send(new Application.Features.PostTradeProcessing.CapitalMarket.Commands.ImportCmTradeFileCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "BHAVCOPY":
                await mediator.Send(new Application.Features.PostTradeProcessing.CapitalMarket.Commands.ImportCmBhavCopyCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "MARGIN":
                await mediator.Send(new Application.Features.PostTradeProcessing.CapitalMarket.Commands.ImportCmMarginCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "OBLIGATION":
                await mediator.Send(new Application.Features.PostTradeProcessing.CapitalMarket.Commands.ImportCmObligationCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "STT":
                await mediator.Send(new Application.Features.PostTradeProcessing.CapitalMarket.Commands.ImportCmSttCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
            case "STAMPDUTY":
                await mediator.Send(new Application.Features.PostTradeProcessing.CapitalMarket.Commands.ImportCmStampDutyCommand(stream, tradingDate, exchange, triggerSource, fileName), ct);
                break;
        }
    }
}
