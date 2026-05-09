using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixFoTradeBookLotAndExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Fix 1: LotSize / NumberOfLots were stored in reverse during initial import.
            // LotSize  = FMULTIPLIER (the per-lot multiplier, e.g. 75 for NIFTY).
            // NumberOfLots = TradQty / LotSize (number of lots traded, e.g. 2).
            //
            // Safe condition for INDEX contracts: standard lot sizes are always >= 15
            // (BANKNIFTY=30, NIFTY=75, FINNIFTY=65, SENSEX=20, BANKEX=15).
            // If stored LotSize < 15 for an index contract, it contains the lot count — swap it.
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook""
                SET
                    ""NumberOfLots"" = ""LotSize"",
                    ""LotSize""      = ROUND(""NumberOfLots"")::bigint
                WHERE ""ContractType"" IN ('FUTIDX', 'OPTIDX')
                  AND ""LotSize"" > 0
                  AND ""LotSize"" < 15
                  AND ""NumberOfLots"" > ""LotSize"";
            ");

            // ── Fix 2: Backfill ExpiryDate in FoTradeBook from raw XpryDt in FoTrades.
            // Covers all date formats observed in NSE/BSE FO trade files.

            // 13-digit epoch milliseconds (NSE standard, e.g. "1743446400000")
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook"" tb
                SET ""ExpiryDate"" = (to_timestamp(ft.""XpryDt""::bigint / 1000.0) AT TIME ZONE 'UTC')::date
                FROM post_trade.""FoTrades"" ft
                WHERE ft.""BatchId""       = tb.""BatchId""
                  AND ft.""UniqueTradeId"" = tb.""UniqueTradeId""
                  AND tb.""ExpiryDate"" IS NULL
                  AND ft.""XpryDt""  IS NOT NULL
                  AND ft.""XpryDt""  ~ '^\d{13}$';
            ");

            // 10-digit epoch seconds
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook"" tb
                SET ""ExpiryDate"" = (to_timestamp(ft.""XpryDt""::bigint) AT TIME ZONE 'UTC')::date
                FROM post_trade.""FoTrades"" ft
                WHERE ft.""BatchId""       = tb.""BatchId""
                  AND ft.""UniqueTradeId"" = tb.""UniqueTradeId""
                  AND tb.""ExpiryDate"" IS NULL
                  AND ft.""XpryDt""  IS NOT NULL
                  AND ft.""XpryDt""  ~ '^\d{10}$';
            ");

            // DD-MMM-YYYY (BSE, e.g. "31-MAR-2026")
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook"" tb
                SET ""ExpiryDate"" = TO_DATE(ft.""XpryDt"", 'DD-Mon-YYYY')
                FROM post_trade.""FoTrades"" ft
                WHERE ft.""BatchId""       = tb.""BatchId""
                  AND ft.""UniqueTradeId"" = tb.""UniqueTradeId""
                  AND tb.""ExpiryDate"" IS NULL
                  AND ft.""XpryDt""  IS NOT NULL
                  AND ft.""XpryDt""  ~ '^\d{2}-[A-Za-z]{3}-\d{4}$';
            ");

            // YYYY-MM-DD (ISO)
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook"" tb
                SET ""ExpiryDate"" = TO_DATE(ft.""XpryDt"", 'YYYY-MM-DD')
                FROM post_trade.""FoTrades"" ft
                WHERE ft.""BatchId""       = tb.""BatchId""
                  AND ft.""UniqueTradeId"" = tb.""UniqueTradeId""
                  AND tb.""ExpiryDate"" IS NULL
                  AND ft.""XpryDt""  IS NOT NULL
                  AND ft.""XpryDt""  ~ '^\d{4}-\d{2}-\d{2}$';
            ");

            // DD-MM-YYYY (e.g. "31-03-2026")
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook"" tb
                SET ""ExpiryDate"" = TO_DATE(ft.""XpryDt"", 'DD-MM-YYYY')
                FROM post_trade.""FoTrades"" ft
                WHERE ft.""BatchId""       = tb.""BatchId""
                  AND ft.""UniqueTradeId"" = tb.""UniqueTradeId""
                  AND tb.""ExpiryDate"" IS NULL
                  AND ft.""XpryDt""  IS NOT NULL
                  AND ft.""XpryDt""  ~ '^\d{2}-\d{2}-\d{4}$';
            ");

            // YYYYMMDD compact (e.g. "20260331")
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook"" tb
                SET ""ExpiryDate"" = TO_DATE(ft.""XpryDt"", 'YYYYMMDD')
                FROM post_trade.""FoTrades"" ft
                WHERE ft.""BatchId""       = tb.""BatchId""
                  AND ft.""UniqueTradeId"" = tb.""UniqueTradeId""
                  AND tb.""ExpiryDate"" IS NULL
                  AND ft.""XpryDt""  IS NOT NULL
                  AND ft.""XpryDt""  ~ '^\d{8}$';
            ");

            // Sync ExpiryDate in FoTrades staging table as well
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTrades""
                SET ""ExpiryDate"" = (to_timestamp(""XpryDt""::bigint / 1000.0) AT TIME ZONE 'UTC')::date
                WHERE ""ExpiryDate"" IS NULL AND ""XpryDt"" IS NOT NULL AND ""XpryDt"" ~ '^\d{13}$';

                UPDATE post_trade.""FoTrades""
                SET ""ExpiryDate"" = (to_timestamp(""XpryDt""::bigint) AT TIME ZONE 'UTC')::date
                WHERE ""ExpiryDate"" IS NULL AND ""XpryDt"" IS NOT NULL AND ""XpryDt"" ~ '^\d{10}$';

                UPDATE post_trade.""FoTrades""
                SET ""ExpiryDate"" = TO_DATE(""XpryDt"", 'DD-Mon-YYYY')
                WHERE ""ExpiryDate"" IS NULL AND ""XpryDt"" IS NOT NULL AND ""XpryDt"" ~ '^\d{2}-[A-Za-z]{3}-\d{4}$';

                UPDATE post_trade.""FoTrades""
                SET ""ExpiryDate"" = TO_DATE(""XpryDt"", 'YYYY-MM-DD')
                WHERE ""ExpiryDate"" IS NULL AND ""XpryDt"" IS NOT NULL AND ""XpryDt"" ~ '^\d{4}-\d{2}-\d{2}$';

                UPDATE post_trade.""FoTrades""
                SET ""ExpiryDate"" = TO_DATE(""XpryDt"", 'DD-MM-YYYY')
                WHERE ""ExpiryDate"" IS NULL AND ""XpryDt"" IS NOT NULL AND ""XpryDt"" ~ '^\d{2}-\d{2}-\d{4}$';

                UPDATE post_trade.""FoTrades""
                SET ""ExpiryDate"" = TO_DATE(""XpryDt"", 'YYYYMMDD')
                WHERE ""ExpiryDate"" IS NULL AND ""XpryDt"" IS NOT NULL AND ""XpryDt"" ~ '^\d{8}$';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only migration — not reversible
        }
    }
}
