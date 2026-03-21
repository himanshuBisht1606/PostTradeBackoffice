using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoFileImportBatch : BaseEntity
{
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }
    public FoFileType FileType { get; set; }
    public string Exchange { get; set; } = string.Empty;   // NFO | BFO
    public DateOnly TradingDate { get; set; }
    public FoImportStatus Status { get; set; }
    public FoTriggerSource TriggerSource { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int CreatedRows { get; set; }
    public int SkippedRows { get; set; }
    public int ErrorRows { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public virtual ICollection<FoFileImportLog> Logs { get; set; } = new List<FoFileImportLog>();
}
