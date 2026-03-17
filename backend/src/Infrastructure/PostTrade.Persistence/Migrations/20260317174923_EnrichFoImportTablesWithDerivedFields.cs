using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichFoImportTablesWithDerivedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoTrades",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstrumentType",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "LotSize",
                schema: "post_trade",
                table: "FoTrades",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "NumLots",
                schema: "post_trade",
                table: "FoTrades",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TradeValue",
                schema: "post_trade",
                table: "FoTrades",
                type: "numeric(22,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnderlyingSymbol",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                schema: "post_trade",
                table: "FoStts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoStts",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoStts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                schema: "post_trade",
                table: "FoStampDuties",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoStampDuties",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoStampDuties",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                schema: "post_trade",
                table: "FoPositions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoPositions",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoPositions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoBhavCopies",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstrumentType",
                schema: "post_trade",
                table: "FoBhavCopies",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "InstrumentType",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "LotSize",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "NumLots",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "TradeValue",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "UnderlyingSymbol",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "ClientName",
                schema: "post_trade",
                table: "FoStts");

            migrationBuilder.DropColumn(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoStts");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoStts");

            migrationBuilder.DropColumn(
                name: "ClientName",
                schema: "post_trade",
                table: "FoStampDuties");

            migrationBuilder.DropColumn(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoStampDuties");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoStampDuties");

            migrationBuilder.DropColumn(
                name: "ClientName",
                schema: "post_trade",
                table: "FoPositions");

            migrationBuilder.DropColumn(
                name: "ClientStateCode",
                schema: "post_trade",
                table: "FoPositions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoPositions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoContractMasters");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "post_trade",
                table: "FoBhavCopies");

            migrationBuilder.DropColumn(
                name: "InstrumentType",
                schema: "post_trade",
                table: "FoBhavCopies");
        }
    }
}
