using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoTradeBookMandatoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CtclId",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrgnlCtdnPtcptId",
                schema: "post_trade",
                table: "FoTrades",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CtclId",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalClientId",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            // Backfill: convert English ContractType names to short codes
            migrationBuilder.Sql(@"
                UPDATE post_trade.""FoTradeBook""
                SET ""ContractType"" = CASE ""ContractType""
                    WHEN 'Index Future' THEN 'FUTIDX'
                    WHEN 'Stock Future' THEN 'FUTSTK'
                    WHEN 'Index Option' THEN 'OPTIDX'
                    WHEN 'Stock Option' THEN 'OPTSTK'
                    ELSE ""ContractType""
                END;

                UPDATE post_trade.""FoTrades""
                SET ""InstrumentType"" = CASE ""InstrumentType""
                    WHEN 'Index Future' THEN 'FUTIDX'
                    WHEN 'Stock Future' THEN 'FUTSTK'
                    WHEN 'Index Option' THEN 'OPTIDX'
                    WHEN 'Stock Option' THEN 'OPTSTK'
                    ELSE ""InstrumentType""
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CtclId",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "OrgnlCtdnPtcptId",
                schema: "post_trade",
                table: "FoTrades");

            migrationBuilder.DropColumn(
                name: "CtclId",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "OriginalClientId",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "Segment",
                schema: "post_trade",
                table: "FoTradeBook");
        }
    }
}
