using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmFileImportBatch : BaseEntity
{
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }
    public CmFileType FileType { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public DateOnly TradingDate { get; set; }
    public CmImportStatus Status { get; set; }
    public CmTriggerSource TriggerSource { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int CreatedRows { get; set; }
    public int SkippedRows { get; set; }
    public int ErrorRows { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public virtual ICollection<CmFileImportLog> Logs { get; set; } = new List<CmFileImportLog>();
}
