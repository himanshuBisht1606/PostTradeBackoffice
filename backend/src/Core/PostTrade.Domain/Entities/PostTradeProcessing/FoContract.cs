namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Curated FO contracts master — equivalent of CFORise CONTRACTS table.
/// Populated from FoContractMaster (staging) after filtering invalid rows.
/// Used by trade import for enrichment (LotSize, FMultiplier, ContractName).
/// </summary>
public class FoContract : BaseEntity
{
    public Guid ContractId { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceBatchId { get; set; }     // FoFileImportBatch that created this

    public string Exchange { get; set; } = string.Empty;       // NFO | BFO
    public DateOnly TradingDate { get; set; }                  // Date when imported

    // Contract identity
    public string InstrumentType { get; set; } = string.Empty; // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    public string Symbol { get; set; } = string.Empty;         // TckrSymb
    public string ContractName { get; set; } = string.Empty;   // UPPER(InstrType+Symbol+ExpiryDate_DDMONYYYY)

    // Contract attributes
    public DateOnly ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }                   // Raw value from exchange file
    public string OptionType { get; set; } = string.Empty;     // CE | PE | FX
    public long LotSize { get; set; }                          // MinLot
    public decimal FMultiplier { get; set; } = 1m;             // Mltplr (f[71]) — CMULTIPLIER, default 1

    // Reference / enrichment fields
    public string? FinInstrmId { get; set; }                   // Exchange instrument token (for lookup)
    public string UnderlyingSymbol { get; set; } = string.Empty;
    public string? Isin { get; set; }
    public decimal TickSize { get; set; }
    public string? SttlmMtd { get; set; }

    // Registration
    public Guid? RegisteredInstrumentId { get; set; }  // set when registered as master Instrument
}
