using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoStructuredTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoClientPositionBook",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClearingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BrokerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientStateCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    LotSize = table.Column<long>(type: "bigint", nullable: false),
                    OpenLongQty = table.Column<long>(type: "bigint", nullable: false),
                    OpenLongValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    OpenShortQty = table.Column<long>(type: "bigint", nullable: false),
                    OpenShortValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    DayBuyQty = table.Column<long>(type: "bigint", nullable: false),
                    DayBuyValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    DaySellQty = table.Column<long>(type: "bigint", nullable: false),
                    DaySellValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    PreExerciseLongQty = table.Column<long>(type: "bigint", nullable: false),
                    PreExerciseLongValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    PreExerciseShortQty = table.Column<long>(type: "bigint", nullable: false),
                    PreExerciseShortValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    ExercisedQty = table.Column<long>(type: "bigint", nullable: false),
                    AssignedQty = table.Column<long>(type: "bigint", nullable: false),
                    PostExerciseLongQty = table.Column<long>(type: "bigint", nullable: false),
                    PostExerciseLongValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    PostExerciseShortQty = table.Column<long>(type: "bigint", nullable: false),
                    PostExerciseShortValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    SettlementPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ReferenceRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DailyMtmSettlement = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    FuturesFinalSettlement = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExerciseAssignmentValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_FoClientPositionBook", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoClientPositionBook_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoDailyMarketData",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    InstrumentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InstrumentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    LotSize = table.Column<long>(type: "bigint", nullable: false),
                    OpenPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    HighPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LowPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ClosePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LastTradedPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PreviousClose = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnderlyingPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SettlementPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OpenInterest = table.Column<long>(type: "bigint", nullable: false),
                    OpenInterestChange = table.Column<long>(type: "bigint", nullable: false),
                    TotalVolume = table.Column<long>(type: "bigint", nullable: false),
                    TotalTurnover = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TotalTrades = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_FoDailyMarketData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoDailyMarketData_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoStampDutyLedger",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClearingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BrokerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientStateCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    StateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    SettlementPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalBuyQty = table.Column<long>(type: "bigint", nullable: false),
                    TotalBuyValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TotalSellQty = table.Column<long>(type: "bigint", nullable: false),
                    TotalSellValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    DeliveryBuyQty = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryBuyValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    NonDeliveryBuyQty = table.Column<long>(type: "bigint", nullable: false),
                    NonDeliveryBuyValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    BuyStampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SellStampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DeliveryBuyStampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NonDeliveryBuyStampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalStampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_FoStampDutyLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoStampDutyLedger_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoSttLedger",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClearingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BrokerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientStateCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    SettlementPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalBuyQty = table.Column<long>(type: "bigint", nullable: false),
                    TotalBuyValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TotalSellQty = table.Column<long>(type: "bigint", nullable: false),
                    TotalSellValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TaxableSellFuturesValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TaxableSellOptionValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    OptionExerciseQty = table.Column<long>(type: "bigint", nullable: false),
                    OptionExerciseValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TaxableExerciseValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    FuturesStt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionsStt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalStt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_FoSttLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoSttLedger_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoTradeBook",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    UniqueTradeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrokerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InstrumentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InstrumentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UnderlyingSymbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    LotSize = table.Column<long>(type: "bigint", nullable: false),
                    ClientType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientStateCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Side = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    NumberOfLots = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TradeValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    SettlementType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SettlementTransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_FoTradeBook", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoTradeBook_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoClientPositionBook_BatchId",
                schema: "post_trade",
                table: "FoClientPositionBook",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoClientPositionBook_TenantId_BatchId",
                schema: "post_trade",
                table: "FoClientPositionBook",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoClientPositionBook_TenantId_TradeDate_Exchange_ClientCode",
                schema: "post_trade",
                table: "FoClientPositionBook",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "ClientCode" });

            migrationBuilder.CreateIndex(
                name: "IX_FoDailyMarketData_BatchId",
                schema: "post_trade",
                table: "FoDailyMarketData",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoDailyMarketData_TenantId_BatchId",
                schema: "post_trade",
                table: "FoDailyMarketData",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoDailyMarketData_TenantId_TradeDate_Exchange_Symbol",
                schema: "post_trade",
                table: "FoDailyMarketData",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "Symbol" });

            migrationBuilder.CreateIndex(
                name: "IX_FoStampDutyLedger_BatchId",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoStampDutyLedger_TenantId_BatchId",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoStampDutyLedger_TenantId_TradeDate_Exchange_ClientCode",
                schema: "post_trade",
                table: "FoStampDutyLedger",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "ClientCode" });

            migrationBuilder.CreateIndex(
                name: "IX_FoSttLedger_BatchId",
                schema: "post_trade",
                table: "FoSttLedger",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoSttLedger_TenantId_BatchId",
                schema: "post_trade",
                table: "FoSttLedger",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoSttLedger_TenantId_TradeDate_Exchange_ClientCode",
                schema: "post_trade",
                table: "FoSttLedger",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "ClientCode" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeBook_BatchId",
                schema: "post_trade",
                table: "FoTradeBook",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeBook_TenantId_BatchId_UniqueTradeId",
                schema: "post_trade",
                table: "FoTradeBook",
                columns: new[] { "TenantId", "BatchId", "UniqueTradeId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeBook_TenantId_ClientCode_TradeDate",
                schema: "post_trade",
                table: "FoTradeBook",
                columns: new[] { "TenantId", "ClientCode", "TradeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeBook_TenantId_TradeDate_Exchange_Symbol",
                schema: "post_trade",
                table: "FoTradeBook",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "Symbol" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoClientPositionBook",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoDailyMarketData",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoStampDutyLedger",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoSttLedger",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoTradeBook",
                schema: "post_trade");
        }
    }
}
