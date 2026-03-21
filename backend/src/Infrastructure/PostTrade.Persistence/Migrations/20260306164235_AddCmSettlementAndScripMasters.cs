using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCmSettlementAndScripMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmScripMasters",
                schema: "post_trade",
                columns: table => new
                {
                    CmScripMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ISIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Series = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FaceValue = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    LotSize = table.Column<int>(type: "integer", nullable: false),
                    TickSize = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    InstrumentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_CmScripMasters", x => x.CmScripMasterId);
                });

            migrationBuilder.CreateTable(
                name: "CmSettlementMasters",
                schema: "post_trade",
                columns: table => new
                {
                    CmSettlementMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SettlementNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SettlementType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PayOutDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_CmSettlementMasters", x => x.CmSettlementMasterId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmScripMasters_TenantId_Exchange_TradingDate",
                schema: "post_trade",
                table: "CmScripMasters",
                columns: new[] { "TenantId", "Exchange", "TradingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CmScripMasters_TenantId_Exchange_TradingDate_ISIN",
                schema: "post_trade",
                table: "CmScripMasters",
                columns: new[] { "TenantId", "Exchange", "TradingDate", "ISIN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmSettlementMasters_TenantId_Exchange_TradingDate_Settlemen~",
                schema: "post_trade",
                table: "CmSettlementMasters",
                columns: new[] { "TenantId", "Exchange", "TradingDate", "SettlementNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmScripMasters",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmSettlementMasters",
                schema: "post_trade");
        }
    }
}
