using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixFoTradeDateUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FoTradeDate_TenantId_TradeDate_Exchange_UniqueTradeId",
                schema: "post_trade",
                table: "FoTradeDate");

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeDate_TenantId_TradeDate_Exchange_UniqueTradeId_Clien~",
                schema: "post_trade",
                table: "FoTradeDate",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "UniqueTradeId", "ClientCode", "Side" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FoTradeDate_TenantId_TradeDate_Exchange_UniqueTradeId_Clien~",
                schema: "post_trade",
                table: "FoTradeDate");

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeDate_TenantId_TradeDate_Exchange_UniqueTradeId",
                schema: "post_trade",
                table: "FoTradeDate",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "UniqueTradeId" },
                unique: true);
        }
    }
}
