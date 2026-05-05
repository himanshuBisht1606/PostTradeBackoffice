using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoContractAndTradeDateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "fo_trn_slno_seq",
                schema: "post_trade");

            migrationBuilder.AddColumn<decimal>(
                name: "FMultiplier",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "numeric(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "GlobalExchange",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCustodianTrade",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrdType",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "TrnSlNo",
                schema: "post_trade",
                table: "FoTradeBook",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "FMultiplier",
                schema: "post_trade",
                table: "FoContractMasters",
                type: "numeric(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "BrokerCode",
                schema: "master",
                table: "ExchangeSegments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearingCorp",
                schema: "master",
                table: "ExchangeSegments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalExchangeCode",
                schema: "master",
                table: "ExchangeSegments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradingMemberId",
                schema: "master",
                table: "ExchangeSegments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FoContracts",
                schema: "post_trade",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InstrumentType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    LotSize = table.Column<long>(type: "bigint", nullable: false),
                    FMultiplier = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    FinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnderlyingSymbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TickSize = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttlmMtd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("PK_FoContracts", x => x.ContractId);
                });

            migrationBuilder.CreateTable(
                name: "FoTradeDate",
                schema: "post_trade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrnSlNo = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('post_trade.fo_trn_slno_seq')"),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    GlobalExchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Segment = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    UniqueTradeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TrdType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ClearingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TradingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InstrumentType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InstrumentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptionType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    UnderlyingSymbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LotSize = table.Column<long>(type: "bigint", nullable: false),
                    FMultiplier = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginalClientCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ClientType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CtclId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientStateCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    IsCustodianTrade = table.Column<bool>(type: "boolean", nullable: false),
                    Side = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    NumberOfLots = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TradeValue = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TradeDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TradeStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OrderRef = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MarketType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BookType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BookTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SettlementType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SettlementTransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SettlementDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CounterpartyCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FoTradeDate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoTradeDate_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeBook_TenantId_GlobalExchange_TradeDate_ClientCode",
                schema: "post_trade",
                table: "FoTradeBook",
                columns: new[] { "TenantId", "GlobalExchange", "TradeDate", "ClientCode" });

            // Backfill TrnSlNo for any existing FoTradeBook rows (all currently have 0).
            // Each existing row gets a unique value from the shared sequence so the
            // unique index below can be created without a 23505 duplicate-key error.
            migrationBuilder.Sql(
                "UPDATE post_trade.\"FoTradeBook\" SET \"TrnSlNo\" = nextval('post_trade.fo_trn_slno_seq');");

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeBook_TenantId_TrnSlNo",
                schema: "post_trade",
                table: "FoTradeBook",
                columns: new[] { "TenantId", "TrnSlNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoContracts_TenantId_Exchange_TradingDate_FinInstrmId",
                schema: "post_trade",
                table: "FoContracts",
                columns: new[] { "TenantId", "Exchange", "TradingDate", "FinInstrmId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoContracts_TenantId_Exchange_TradingDate_Symbol_ExpiryDate",
                schema: "post_trade",
                table: "FoContracts",
                columns: new[] { "TenantId", "Exchange", "TradingDate", "Symbol", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeDate_BatchId",
                schema: "post_trade",
                table: "FoTradeDate",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeDate_TenantId_BatchId",
                schema: "post_trade",
                table: "FoTradeDate",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeDate_TenantId_GlobalExchange_TradeDate_ClientCode",
                schema: "post_trade",
                table: "FoTradeDate",
                columns: new[] { "TenantId", "GlobalExchange", "TradeDate", "ClientCode" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTradeDate_TenantId_TradeDate_Exchange_UniqueTradeId",
                schema: "post_trade",
                table: "FoTradeDate",
                columns: new[] { "TenantId", "TradeDate", "Exchange", "UniqueTradeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoContracts",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoTradeDate",
                schema: "post_trade");

            migrationBuilder.DropIndex(
                name: "IX_FoTradeBook_TenantId_GlobalExchange_TradeDate_ClientCode",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropIndex(
                name: "IX_FoTradeBook_TenantId_TrnSlNo",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "FMultiplier",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "GlobalExchange",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "IsCustodianTrade",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "TrdType",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "TrnSlNo",
                schema: "post_trade",
                table: "FoTradeBook");

            migrationBuilder.DropColumn(
                name: "FMultiplier",
                schema: "post_trade",
                table: "FoContractMasters");

            migrationBuilder.DropColumn(
                name: "BrokerCode",
                schema: "master",
                table: "ExchangeSegments");

            migrationBuilder.DropColumn(
                name: "ClearingCorp",
                schema: "master",
                table: "ExchangeSegments");

            migrationBuilder.DropColumn(
                name: "GlobalExchangeCode",
                schema: "master",
                table: "ExchangeSegments");

            migrationBuilder.DropColumn(
                name: "TradingMemberId",
                schema: "master",
                table: "ExchangeSegments");

            migrationBuilder.DropSequence(
                name: "fo_trn_slno_seq",
                schema: "post_trade");
        }
    }
}
