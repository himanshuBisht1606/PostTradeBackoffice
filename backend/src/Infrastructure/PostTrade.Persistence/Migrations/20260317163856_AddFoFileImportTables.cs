using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoFileImportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoFileImportBatches",
                schema: "post_trade",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TriggerSource = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    CreatedRows = table.Column<int>(type: "integer", nullable: false),
                    SkippedRows = table.Column<int>(type: "integer", nullable: false),
                    ErrorRows = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_FoFileImportBatches", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "FoBhavCopies",
                schema: "post_trade",
                columns: table => new
                {
                    BhavCopyRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Sgmt = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Src = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FinInstrmTp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TckrSymb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SctySrs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    XpryDt = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StrkPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptnTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    FinInstrmNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OpnPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    HghPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LwPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ClsPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LastPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PrvsClsgPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UndrlygPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttlmPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OpnIntrst = table.Column<long>(type: "bigint", nullable: false),
                    ChngInOpnIntrst = table.Column<long>(type: "bigint", nullable: false),
                    TtlTradgVol = table.Column<long>(type: "bigint", nullable: false),
                    TtlTrfVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TtlNbOfTxsExctd = table.Column<long>(type: "bigint", nullable: false),
                    NewBrdLotQty = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_FoBhavCopies", x => x.BhavCopyRowId);
                    table.ForeignKey(
                        name: "FK_FoBhavCopies_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoContractMasters",
                schema: "post_trade",
                columns: table => new
                {
                    ContractRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UndrlygFinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FinInstrmNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TckrSymb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    XpryDt = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StrkPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptnTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    FinInstrmTp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SttlmMtd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    StockNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinLot = table.Column<long>(type: "bigint", nullable: false),
                    NewBrdLotQty = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_FoContractMasters", x => x.ContractRowId);
                    table.ForeignKey(
                        name: "FK_FoContractMasters_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoFileImportLogs",
                schema: "post_trade",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RawData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_FoFileImportLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_FoFileImportLogs_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoPositions",
                schema: "post_trade",
                columns: table => new
                {
                    PositionRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    Sgmt = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Src = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClrMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinInstrmTp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TckrSymb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    XpryDt = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StrkPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptnTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    NewBrdLotQty = table.Column<long>(type: "bigint", nullable: false),
                    OpngLngQty = table.Column<long>(type: "bigint", nullable: false),
                    OpngLngVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    OpngShrtQty = table.Column<long>(type: "bigint", nullable: false),
                    OpngShrtVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    OpnBuyTradgQty = table.Column<long>(type: "bigint", nullable: false),
                    OpnBuyTradgVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    OpnSellTradgQty = table.Column<long>(type: "bigint", nullable: false),
                    OpnSellTradgVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    PreExrcAssgndLngQty = table.Column<long>(type: "bigint", nullable: false),
                    PreExrcAssgndLngVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    PreExrcAssgndShrtQty = table.Column<long>(type: "bigint", nullable: false),
                    PreExrcAssgndShrtVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    ExrcdQty = table.Column<long>(type: "bigint", nullable: false),
                    AssgndQty = table.Column<long>(type: "bigint", nullable: false),
                    PstExrcAssgndLngQty = table.Column<long>(type: "bigint", nullable: false),
                    PstExrcAssgndLngVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    PstExrcAssgndShrtQty = table.Column<long>(type: "bigint", nullable: false),
                    PstExrcAssgndShrtVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    SttlmPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    RefRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PrmAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DalyMrkToMktSettlmVal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    FutrsFnlSttlmVal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExrcAssgndVal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_FoPositions", x => x.PositionRowId);
                    table.ForeignKey(
                        name: "FK_FoPositions_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoStampDuties",
                schema: "post_trade",
                columns: table => new
                {
                    StampDutyRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RptHdr = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    Sgmt = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Src = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClrMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    CtrySubDvsn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TckrSymb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FinInstrmTp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    XpryDt = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StrkPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptnTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TtlBuyTradgVol = table.Column<long>(type: "bigint", nullable: false),
                    TtlBuyTrfVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TtlSellTradgVol = table.Column<long>(type: "bigint", nullable: false),
                    TtlSellTrfVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    BuyDlvryQty = table.Column<long>(type: "bigint", nullable: false),
                    BuyDlvryVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    BuyOthrThanDlvryQty = table.Column<long>(type: "bigint", nullable: false),
                    BuyOthrThanDlvryVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    BuyStmpDty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SellStmpDty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttlmPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    BuyDlvryStmpDty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    BuyOthrThanDlvryStmpDty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    StmpDtyAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_FoStampDuties", x => x.StampDutyRowId);
                    table.ForeignKey(
                        name: "FK_FoStampDuties_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoStts",
                schema: "post_trade",
                columns: table => new
                {
                    SttRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RptHdr = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    Sgmt = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Src = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClrMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    TckrSymb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FinInstrmTp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    XpryDt = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OptnTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    StrkPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttlmPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TtlBuyTradgVol = table.Column<long>(type: "bigint", nullable: false),
                    TtlBuyTrfVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TtlSellTradgVol = table.Column<long>(type: "bigint", nullable: false),
                    TtlSellTrfVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TaxblSellFutrsVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TaxblSellOptnVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    OptnExrcQty = table.Column<long>(type: "bigint", nullable: false),
                    OptnExrcVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    TaxblExrcVal = table.Column<decimal>(type: "numeric(22,4)", nullable: false),
                    FutrsTtlTaxs = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptnTtlTaxs = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TtlTaxs = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_FoStts", x => x.SttRowId);
                    table.ForeignKey(
                        name: "FK_FoStts_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoTrades",
                schema: "post_trade",
                columns: table => new
                {
                    TradeRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniqueTradeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    Sgmt = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Src = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FinInstrmTp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TckrSymb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    XpryDt = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StrkPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OptnTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    FinInstrmNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClntTp = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    BuySellInd = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TradQty = table.Column<long>(type: "bigint", nullable: false),
                    NewBrdLotQty = table.Column<long>(type: "bigint", nullable: false),
                    Pric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttlmTp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SctiesSttlmTxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_FoTrades", x => x.TradeRowId);
                    table.ForeignKey(
                        name: "FK_FoTrades_FoFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "FoFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoBhavCopies_BatchId",
                schema: "post_trade",
                table: "FoBhavCopies",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoBhavCopies_TenantId_TradDt_Exchange_TckrSymb",
                schema: "post_trade",
                table: "FoBhavCopies",
                columns: new[] { "TenantId", "TradDt", "Exchange", "TckrSymb" });

            migrationBuilder.CreateIndex(
                name: "IX_FoContractMasters_BatchId",
                schema: "post_trade",
                table: "FoContractMasters",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoContractMasters_TenantId_Exchange_TradingDate_TckrSymb",
                schema: "post_trade",
                table: "FoContractMasters",
                columns: new[] { "TenantId", "Exchange", "TradingDate", "TckrSymb" });

            migrationBuilder.CreateIndex(
                name: "IX_FoFileImportBatches_TenantId_FileType_Exchange_TradingDate",
                schema: "post_trade",
                table: "FoFileImportBatches",
                columns: new[] { "TenantId", "FileType", "Exchange", "TradingDate" },
                unique: true,
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_FoFileImportLogs_BatchId",
                schema: "post_trade",
                table: "FoFileImportLogs",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoPositions_BatchId",
                schema: "post_trade",
                table: "FoPositions",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoPositions_TenantId_BatchId_ClntId_TckrSymb",
                schema: "post_trade",
                table: "FoPositions",
                columns: new[] { "TenantId", "BatchId", "ClntId", "TckrSymb" });

            migrationBuilder.CreateIndex(
                name: "IX_FoStampDuties_BatchId",
                schema: "post_trade",
                table: "FoStampDuties",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoStampDuties_TenantId_BatchId_ClntId_TckrSymb",
                schema: "post_trade",
                table: "FoStampDuties",
                columns: new[] { "TenantId", "BatchId", "ClntId", "TckrSymb" });

            migrationBuilder.CreateIndex(
                name: "IX_FoStts_BatchId",
                schema: "post_trade",
                table: "FoStts",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoStts_TenantId_BatchId_ClntId_TckrSymb",
                schema: "post_trade",
                table: "FoStts",
                columns: new[] { "TenantId", "BatchId", "ClntId", "TckrSymb" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTrades_BatchId",
                schema: "post_trade",
                table: "FoTrades",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FoTrades_TenantId_BatchId_UniqueTradeId",
                schema: "post_trade",
                table: "FoTrades",
                columns: new[] { "TenantId", "BatchId", "UniqueTradeId" });

            migrationBuilder.CreateIndex(
                name: "IX_FoTrades_TenantId_TradDt_Exchange_TckrSymb",
                schema: "post_trade",
                table: "FoTrades",
                columns: new[] { "TenantId", "TradDt", "Exchange", "TckrSymb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoBhavCopies",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoContractMasters",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoFileImportLogs",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoPositions",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoStampDuties",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoStts",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoTrades",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "FoFileImportBatches",
                schema: "post_trade");
        }
    }
}
