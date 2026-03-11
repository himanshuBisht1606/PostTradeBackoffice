using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCmFileImportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "post_trade");

            migrationBuilder.CreateTable(
                name: "CmFileImportBatches",
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
                    table.PrimaryKey("PK_CmFileImportBatches", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "CmBhavCopies",
                schema: "post_trade",
                columns: table => new
                {
                    BhavCopyRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    FinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FinInstrmNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Isin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SctySrs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OpnPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    HghPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LwPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ClsPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LastPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PrvClsgPric = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TtlTradgVol = table.Column<long>(type: "bigint", nullable: false),
                    TtlTrfVal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MktCpzn = table.Column<decimal>(type: "numeric(22,2)", nullable: true),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_CmBhavCopies", x => x.BhavCopyRowId);
                    table.ForeignKey(
                        name: "FK_CmBhavCopies_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmFileImportLogs",
                schema: "post_trade",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RawData = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_CmFileImportLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_CmFileImportLogs_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmMargins",
                schema: "post_trade",
                columns: table => new
                {
                    MarginRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sgmt = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScripNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MtmMrgnAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    VrMrgnAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExpsrMrgnAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    AddhcMrgnAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CrystldLssAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TtlMrgnAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_CmMargins", x => x.MarginRowId);
                    table.ForeignKey(
                        name: "FK_CmMargins_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmObligations",
                schema: "post_trade",
                columns: table => new
                {
                    ObligationRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    SttlmId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SttlmDt = table.Column<DateOnly>(type: "date", nullable: false),
                    IsinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScripNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ObligTyp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NetQty = table.Column<long>(type: "bigint", nullable: false),
                    ObligStdAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CrObligStdAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DrObligStdAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_CmObligations", x => x.ObligationRowId);
                    table.ForeignKey(
                        name: "FK_CmObligations_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmStampDuties",
                schema: "post_trade",
                columns: table => new
                {
                    StampDutyRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RptHdr = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sgmt = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScripNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuySellInd = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TradQty = table.Column<long>(type: "bigint", nullable: false),
                    TradVal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    StmpDtyAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    StmpDtyRate = table.Column<decimal>(type: "numeric(10,6)", nullable: false),
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
                    table.PrimaryKey("PK_CmStampDuties", x => x.StampDutyRowId);
                    table.ForeignKey(
                        name: "FK_CmStampDuties_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmStts",
                schema: "post_trade",
                columns: table => new
                {
                    SttRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RptHdr = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sgmt = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScripNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuySellInd = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TradQty = table.Column<long>(type: "bigint", nullable: false),
                    TradVal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttTaxAmt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttRate = table.Column<decimal>(type: "numeric(10,6)", nullable: false),
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
                    table.PrimaryKey("PK_CmStts", x => x.SttRowId);
                    table.ForeignKey(
                        name: "FK_CmStts_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmTrades",
                schema: "post_trade",
                columns: table => new
                {
                    TradeRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniqueTradeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TradDt = table.Column<DateOnly>(type: "date", nullable: false),
                    Sgmt = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Src = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FinInstrmId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FinInstrmNm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TradngMmbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClntId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrdId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BuySellInd = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    TradQty = table.Column<long>(type: "bigint", nullable: false),
                    PricePrUnit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TradVal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SttlmId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SttlmTyp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_CmTrades", x => x.TradeRowId);
                    table.ForeignKey(
                        name: "FK_CmTrades_CmFileImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "post_trade",
                        principalTable: "CmFileImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmBhavCopies_BatchId",
                schema: "post_trade",
                table: "CmBhavCopies",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmBhavCopies_TenantId_BatchId_FinInstrmId",
                schema: "post_trade",
                table: "CmBhavCopies",
                columns: new[] { "TenantId", "BatchId", "FinInstrmId" });

            migrationBuilder.CreateIndex(
                name: "IX_CmFileImportBatches_TenantId_FileType_Exchange_TradingDate",
                schema: "post_trade",
                table: "CmFileImportBatches",
                columns: new[] { "TenantId", "FileType", "Exchange", "TradingDate" },
                unique: true,
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_CmFileImportLogs_BatchId",
                schema: "post_trade",
                table: "CmFileImportLogs",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmMargins_BatchId",
                schema: "post_trade",
                table: "CmMargins",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmMargins_TenantId_BatchId_ClntId",
                schema: "post_trade",
                table: "CmMargins",
                columns: new[] { "TenantId", "BatchId", "ClntId" });

            migrationBuilder.CreateIndex(
                name: "IX_CmObligations_BatchId",
                schema: "post_trade",
                table: "CmObligations",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmObligations_TenantId_BatchId_ClntId",
                schema: "post_trade",
                table: "CmObligations",
                columns: new[] { "TenantId", "BatchId", "ClntId" });

            migrationBuilder.CreateIndex(
                name: "IX_CmStampDuties_BatchId",
                schema: "post_trade",
                table: "CmStampDuties",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmStampDuties_TenantId_BatchId",
                schema: "post_trade",
                table: "CmStampDuties",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_CmStts_BatchId",
                schema: "post_trade",
                table: "CmStts",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmStts_TenantId_BatchId",
                schema: "post_trade",
                table: "CmStts",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_CmTrades_BatchId",
                schema: "post_trade",
                table: "CmTrades",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CmTrades_TenantId_BatchId_UniqueTradeId",
                schema: "post_trade",
                table: "CmTrades",
                columns: new[] { "TenantId", "BatchId", "UniqueTradeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmBhavCopies",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmFileImportLogs",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmMargins",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmObligations",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmStampDuties",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmStts",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmTrades",
                schema: "post_trade");

            migrationBuilder.DropTable(
                name: "CmFileImportBatches",
                schema: "post_trade");
        }
    }
}
