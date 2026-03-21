namespace PostTrade.Domain.Enums;

public enum ChargeType
{
    Brokerage       = 1,   // Broker commission
    STT             = 2,   // Securities Transaction Tax
    GST             = 3,   // Goods & Services Tax (18%)
    ExchangeTxn     = 4,   // Exchange transaction charges
    SEBI            = 5,   // SEBI turnover fees
    StampDuty       = 6,   // Stamp duty (state-wise)
    IPFT            = 7,   // Investor Protection Fund Trust (exchange levy)
    DPCharge        = 8,   // Depository Participant charges (per scrip / per debit)
    ClearingCharge  = 9,   // Clearing member / clearing house levy
    Other           = 10,  // Any other regulatory or custom charge
}
