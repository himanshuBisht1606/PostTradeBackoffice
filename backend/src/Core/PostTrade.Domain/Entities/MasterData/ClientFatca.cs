namespace PostTrade.Domain.Entities.MasterData;

public class ClientFatca : BaseEntity
{
    public Guid ClientFatcaId { get; set; }
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }
    public string TaxCountry { get; set; } = string.Empty;
    public string? Tin { get; set; }
    public bool IsUsPerson { get; set; }
    public string SourceOfWealth { get; set; } = string.Empty;

    public virtual Client Client { get; set; } = null!;
}
