namespace PostTrade.Domain.Enums;

/// <summary>Market segment to which a charge configuration applies.</summary>
public enum TradeSegment
{
    All  = 0,   // Applies to all segments
    CM   = 1,   // Capital Market (equity cash)
    FO   = 2,   // Futures & Options
    CDS  = 3,   // Currency Derivatives
    COM  = 4,   // Commodity
}
