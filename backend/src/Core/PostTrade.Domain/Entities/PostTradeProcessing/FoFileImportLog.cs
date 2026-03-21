namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoFileImportLog : BaseEntity
{
    public Guid LogId { get; set; }
    public Guid BatchId { get; set; }
    public int RowNumber { get; set; }
    public string Level { get; set; } = "Error"; // Error | Warning
    public string Message { get; set; } = string.Empty;
    public string? RawData { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
