using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoFinanceLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoFinanceLedger",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClearingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BrokerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BuyTurnover = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    SellTurnover = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TotalTurnover = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TotalStt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalStampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Brokerage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExchangeTransactionCharges = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SebiCharges = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Ipft = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    GstOnCharges = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalCharges = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DailyMtmSettlement = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetPremium = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    FinalSettlement = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExerciseAssignmentValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoFinanceLedger", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoFinanceLedger_TenantId_ClientId",
                schema: "post_trade",
                table: "FoFinanceLedger",
                columns: new[] { "TenantId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoFinanceLedger_TenantId_TradeDate_Exchange",
                schema: "post_trade",
                table: "FoFinanceLedger",
                columns: new[] { "TenantId", "TradeDate", "Exchange" });

            migrationBuilder.CreateIndex(
                name: "IX_FoFinanceLedger_TenantId_TradeDate_Exchange_ClientCode",
                schema: "post_trade",
                table: "FoFinanceLedger",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "ClientCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoFinanceLedger",
                schema: "post_trade");
        }
    }
}
