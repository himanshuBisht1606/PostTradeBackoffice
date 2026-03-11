namespace PostTrade.Application.Interfaces;

/// <summary>
/// Downloads scrip master files directly from NSE / BSE exchange servers,
/// decompresses them, and returns a ready-to-parse stream.
/// </summary>
public interface IExchangeScripDownloader
{
    /// <summary>
    /// Downloads NSE_CM_security_{ddMMyyyy}.csv.gz from NSE archives and
    /// returns the decompressed CSV stream.
    /// </summary>
    Task<Stream> DownloadNseAsync(DateOnly tradingDate, CancellationToken ct);

    /// <summary>
    /// Downloads scrip.zip from BSE, extracts the SCRIP_*.TXT file inside it,
    /// and returns the pipe-delimited stream.
    /// </summary>
    Task<Stream> DownloadBseAsync(CancellationToken ct);
}
