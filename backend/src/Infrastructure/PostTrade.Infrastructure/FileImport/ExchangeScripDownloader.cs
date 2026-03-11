using System.IO.Compression;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using PostTrade.Application.Interfaces;

namespace PostTrade.Infrastructure.FileImport;

/// <summary>
/// Downloads scrip master files from NSE / BSE exchange websites.
/// NSE: NSE_CM_security_DDMMYYYY.csv.gz  →  decompressed CSV
/// BSE: scrip.zip  →  SCRIP_*.TXT  (pipe-delimited)
/// </summary>
public class ExchangeScripDownloader : IExchangeScripDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExchangeScripDownloader> _logger;

    public ExchangeScripDownloader(IHttpClientFactory httpClientFactory, ILogger<ExchangeScripDownloader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Downloads NSE security master for the given trading date.
    /// URL: https://nsearchives.nseindia.com/content/cm/NSE_CM_security_DDMMYYYY.csv.gz
    /// </summary>
    public async Task<Stream> DownloadNseAsync(DateOnly tradingDate, CancellationToken ct)
    {
        var dateStr  = tradingDate.ToString("ddMMyyyy");
        var fileName = $"NSE_CM_security_{dateStr}.csv.gz";
        var url      = $"https://nsearchives.nseindia.com/content/cm/{fileName}";

        _logger.LogInformation("ExchangeScripDownloader: Downloading NSE scrip master from {Url}", url);

        var client   = _httpClientFactory.CreateClient("NseClient");
        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        // Decompress GZip → MemoryStream so caller gets a plain CSV stream
        var ms = new MemoryStream();
        await using (var gzipStream = new GZipStream(
            await response.Content.ReadAsStreamAsync(ct),
            CompressionMode.Decompress))
        {
            await gzipStream.CopyToAsync(ms, ct);
        }

        ms.Position = 0;
        _logger.LogInformation("ExchangeScripDownloader: NSE scrip master downloaded ({Bytes} bytes uncompressed)", ms.Length);
        return ms;
    }

    /// <summary>
    /// Downloads BSE scrip master (always the latest).
    /// URL: https://www.bseindia.com/downloads/Help/file/scrip.zip
    /// Extracts SCRIP_*.TXT from the ZIP.
    /// </summary>
    public async Task<Stream> DownloadBseAsync(CancellationToken ct)
    {
        const string url = "https://www.bseindia.com/downloads/Help/file/scrip.zip";

        _logger.LogInformation("ExchangeScripDownloader: Downloading BSE scrip master from {Url}", url);

        var client   = _httpClientFactory.CreateClient("BseClient");
        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var zipBytes = await response.Content.ReadAsByteArrayAsync(ct);

        // Extract SCRIP_*.TXT from the ZIP archive
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        var entry = archive.Entries.FirstOrDefault(e =>
            e.Name.StartsWith("SCRIP_", StringComparison.OrdinalIgnoreCase) &&
            e.Name.EndsWith(".TXT", StringComparison.OrdinalIgnoreCase));

        if (entry == null)
            throw new FileNotFoundException("SCRIP_*.TXT not found in BSE scrip.zip archive.");

        _logger.LogInformation("ExchangeScripDownloader: Extracting {Entry} from BSE zip", entry.Name);

        var ms = new MemoryStream();
        await using (var entryStream = entry.Open())
        {
            await entryStream.CopyToAsync(ms, ct);
        }

        ms.Position = 0;
        _logger.LogInformation("ExchangeScripDownloader: BSE scrip master extracted ({Bytes} bytes)", ms.Length);
        return ms;
    }
}
