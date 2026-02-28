namespace PostTrade.Domain.Entities.MasterData;

public class ClientAuthorizedSignatory : BaseEntity
{
    public Guid SignatoryId { get; set; }
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Pan { get; set; } = string.Empty;
    public string? Din { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }

    // Navigation
    public virtual Client Client { get; set; } = null!;
}
