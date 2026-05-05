using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichFoStructuredTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookType",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookTypeName",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchCode",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Brokerage",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearingMemberId",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CounterpartyCode",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExerciseAssignmentPrice",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketType",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetPrice",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SettlementDate",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuturesExpiryStt",
                schema: "post_trade",
                table: "FoSttLedger",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OptionsExpiryStt",
                schema: "post_trade",
                table: "FoSttLedger",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuturesExpiryStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuturesStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OptionsExpiryStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OptionsStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StateName",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractName",
                schema: "post_trade",
                table: "FoClientPositionBook",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetPremium",
                schema: "post_trade",
                table: "FoClientPositionBook",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SegmentIndicator",
                schema: "post_trade",
                table: "FoClientPositionBook",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookType",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "BookTypeName",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "BranchCode",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "Brokerage",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "ClearingMemberId",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "CounterpartyCode",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "ExerciseAssignmentPrice",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "MarketType",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "NetPrice",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "SettlementDate",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "FuturesExpiryStt",
                schema: "post_trade",
                table: "FoSttLedger");

            migrationBuilder.DropColumn(
                name: "OptionsExpiryStt",
                schema: "post_trade",
                table: "FoSttLedger");

            migrationBuilder.DropColumn(
                name: "FuturesExpiryStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger");

            migrationBuilder.DropColumn(
                name: "FuturesStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger");

            migrationBuilder.DropColumn(
                name: "OptionsExpiryStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger");

            migrationBuilder.DropColumn(
                name: "OptionsStampDuty",
                schema: "post_trade",
                table: "FoStampDutyLedger");

            migrationBuilder.DropColumn(
                name: "StateName",
                schema: "post_trade",
                table: "FoStampDutyLedger");

            migrationBuilder.DropColumn(
                name: "ContractName",
                schema: "post_trade",
                table: "FoClientPositionBook");

            migrationBuilder.DropColumn(
                name: "NetPremium",
                schema: "post_trade",
                table: "FoClientPositionBook");

            migrationBuilder.DropColumn(
                name: "SegmentIndicator",
                schema: "post_trade",
                table: "FoClientPositionBook");
        }
    }
}
