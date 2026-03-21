using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichFoContractMasterAndMarketData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Segment",
                schema: "post_trade",
                table: "FoDailyMarketData",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePric",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MktTpAndId",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptnExrcStyle",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TickSize",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Segment",
                schema: "post_trade",
                table: "FoDailyMarketData");

            migrationBuilder.DropColumn(
                name: "BasePric",
                schema: "post_trade",
                table: "FoContractMasters");

            migrationBuilder.DropColumn(
                name: "Isin",
                schema: "post_trade",
                table: "FoContractMasters");

            migrationBuilder.DropColumn(
                name: "MktTpAndId",
                schema: "post_trade",
                table: "FoContractMasters");

            migrationBuilder.DropColumn(
                name: "OptnExrcStyle",
                schema: "post_trade",
                table: "FoContractMasters");

            migrationBuilder.DropColumn(
                name: "TickSize",
                schema: "post_trade",
                table: "FoContractMasters");
        }
    }
}
