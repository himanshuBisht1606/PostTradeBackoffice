using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoTradeExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── FoTrades staging table — add extended exchange file fields ──────
            migrationBuilder.AddColumn<string>(
                name: "MktTpandId",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrdrRef",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RptdTxSts",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SttlmCycl",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradDtTm",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            // ── FoTradeBook structured table — add descriptive fields ─────────
            migrationBuilder.AddColumn<string>(
                name: "OrderRef",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TradeDateTime",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeStatus",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            // ── ChargesConfigurations — Segment/ApplicableTo/Remarks ─────────
            // Use ADD COLUMN IF NOT EXISTS so this is safe even if
            // AddChargesConfigSegmentFields migration was already applied.
            migrationBuilder.Sql(@"
                ALTER TABLE ledger.""ChargesConfigurations""
                    ADD COLUMN IF NOT EXISTS ""Segment"" character varying(20) NOT NULL DEFAULT 'All';
                ALTER TABLE ledger.""ChargesConfigurations""
                    ADD COLUMN IF NOT EXISTS ""ApplicableTo"" character varying(20) NOT NULL DEFAULT 'Both';
                ALTER TABLE ledger.""ChargesConfigurations""
                    ADD COLUMN IF NOT EXISTS ""Remarks"" character varying(500) NULL;
                DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Effectiv~"";
                DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_EffectiveFrom"";
                DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Segment_~"";
                CREATE INDEX IF NOT EXISTS ""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Segment_~""
                    ON ledger.""ChargesConfigurations""
                    (""TenantId"", ""BrokerId"", ""ChargeType"", ""Segment"", ""EffectiveFrom"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MktTpandId",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "OrdrRef",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "RptdTxSts",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "SttlmCycl",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "TradDtTm",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "OrderRef",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "Remarks",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "TradeDateTime",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "TradeStatus",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Segment_~"";
                CREATE INDEX IF NOT EXISTS ""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Effectiv~""
                    ON ledger.""ChargesConfigurations""
                    (""TenantId"", ""BrokerId"", ""ChargeType"", ""EffectiveFrom"");
                ALTER TABLE ledger.""ChargesConfigurations"" DROP COLUMN IF EXISTS ""Segment"";
                ALTER TABLE ledger.""ChargesConfigurations"" DROP COLUMN IF EXISTS ""ApplicableTo"";
                ALTER TABLE ledger.""ChargesConfigurations"" DROP COLUMN IF EXISTS ""Remarks"";
            ");
        }
    }
}
