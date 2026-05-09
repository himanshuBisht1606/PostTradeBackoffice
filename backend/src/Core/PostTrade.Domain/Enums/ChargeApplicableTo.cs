namespace PostTrade.Domain.Enums;

/// <summary>Trade side on which a charge applies.</summary>
public enum ChargeApplicableTo
{
    Both = 0,
    Buy  = 1,
    Sell = 2,
}
