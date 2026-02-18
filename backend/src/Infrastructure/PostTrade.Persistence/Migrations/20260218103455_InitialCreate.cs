using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "master");

            migrationBuilder.EnsureSchema(
                name: "ledger");

            migrationBuilder.EnsureSchema(
                name: "corporate");

            migrationBuilder.EnsureSchema(
                name: "trading");

            migrationBuilder.EnsureSchema(
                name: "recon");

            migrationBuilder.EnsureSchema(
                name: "settlement");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "audit",
                columns: table => new
                {
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuditType = table.Column<int>(type: "integer", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "ChargesConfigurations",
                schema: "ledger",
                columns: table => new
                {
                    ChargesConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChargeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChargeType = table.Column<int>(type: "integer", nullable: false),
                    CalculationType = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    MaxAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargesConfigurations", x => x.ChargesConfigId);
                });

            migrationBuilder.CreateTable(
                name: "CorporateActions",
                schema: "corporate",
                columns: table => new
                {
                    CorporateActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    AnnouncementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DividendAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    BonusRatio = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    SplitRatio = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    RightsRatio = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    RightsPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateActions", x => x.CorporateActionId);
                });

            migrationBuilder.CreateTable(
                name: "LedgerEntries",
                schema: "ledger",
                columns: table => new
                {
                    LedgerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LedgerType = table.Column<int>(type: "integer", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Narration = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    ReversalLedgerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntries", x => x.LedgerId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "master",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "PnLSnapshots",
                schema: "trading",
                columns: table => new
                {
                    PnLId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SnapshotTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RealizedPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnrealizedPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Brokerage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Taxes = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OpenQuantity = table.Column<int>(type: "integer", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    MarketPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_PnLSnapshots", x => x.PnLId);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "trading",
                columns: table => new
                {
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BuyQuantity = table.Column<int>(type: "integer", nullable: false),
                    SellQuantity = table.Column<int>(type: "integer", nullable: false),
                    NetQuantity = table.Column<int>(type: "integer", nullable: false),
                    CarryForwardQuantity = table.Column<int>(type: "integer", nullable: false),
                    AverageBuyPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    AverageSellPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LastTradePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    MarketPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    RealizedPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnrealizedPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DayPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalPnL = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    BuyValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SellValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                });

            migrationBuilder.CreateTable(
                name: "Reconciliations",
                schema: "recon",
                columns: table => new
                {
                    ReconId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SettlementNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReconType = table.Column<int>(type: "integer", nullable: false),
                    SystemValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExchangeValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ToleranceLimit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reconciliations", x => x.ReconId);
                });

            migrationBuilder.CreateTable(
                name: "SettlementBatches",
                schema: "settlement",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TradeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalTrades = table.Column<int>(type: "integer", nullable: false),
                    TotalTurnover = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementBatches", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "master",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LicenseKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                schema: "trading",
                columns: table => new
                {
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExchangeTradeNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TradeValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TradeTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SettlementNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsAmended = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalTradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Brokerage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    STT = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExchangeTxnCharge = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    GST = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SEBITurnoverCharge = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    StampDuty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalCharges = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.TradeId);
                });

            migrationBuilder.CreateTable(
                name: "ReconExceptions",
                schema: "recon",
                columns: table => new
                {
                    ExceptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionType = table.Column<int>(type: "integer", nullable: false),
                    ExceptionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReferenceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconExceptions", x => x.ExceptionId);
                    table.ForeignKey(
                        name: "FK_ReconExceptions_Reconciliations_ReconId",
                        column: x => x.ReconId,
                        principalSchema: "recon",
                        principalTable: "Reconciliations",
                        principalColumn: "ReconId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementObligations",
                schema: "settlement",
                columns: table => new
                {
                    ObligationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FundsPayIn = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    FundsPayOut = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    NetFundsObligation = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SecuritiesPayIn = table.Column<int>(type: "integer", nullable: false),
                    SecuritiesPayOut = table.Column<int>(type: "integer", nullable: false),
                    NetSecuritiesObligation = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SettledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementObligations", x => x.ObligationId);
                    table.ForeignKey(
                        name: "FK_SettlementObligations_SettlementBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "settlement",
                        principalTable: "SettlementBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Brokers",
                schema: "master",
                columns: table => new
                {
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BrokerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SEBIRegistrationNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PAN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GST = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brokers", x => x.BrokerId);
                    table.ForeignKey(
                        name: "FK_Brokers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                schema: "master",
                columns: table => new
                {
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExchangeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TradingStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TradingEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Exchanges", x => x.ExchangeId);
                    table.ForeignKey(
                        name: "FK_Exchanges_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "master",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Roles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "master",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsMFAEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MFASecret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "master",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PAN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DPId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankIFSC = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                    table.ForeignKey(
                        name: "FK_Clients_Brokers_BrokerId",
                        column: x => x.BrokerId,
                        principalSchema: "master",
                        principalTable: "Brokers",
                        principalColumn: "BrokerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clients_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Segments",
                schema: "master",
                columns: table => new
                {
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SegmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Segments", x => x.SegmentId);
                    table.ForeignKey(
                        name: "FK_Segments_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalSchema: "master",
                        principalTable: "Exchanges",
                        principalColumn: "ExchangeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Segments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "master",
                columns: table => new
                {
                    RolePermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "master",
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "master",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "master",
                columns: table => new
                {
                    UserRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "master",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "master",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instruments",
                schema: "master",
                columns: table => new
                {
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InstrumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ISIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentType = table.Column<int>(type: "integer", nullable: false),
                    LotSize = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TickSize = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Series = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    OptionType = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuditTrail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.InstrumentId);
                    table.ForeignKey(
                        name: "FK_Instruments_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalSchema: "master",
                        principalTable: "Exchanges",
                        principalColumn: "ExchangeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Instruments_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalSchema: "master",
                        principalTable: "Segments",
                        principalColumn: "SegmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Instruments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_EntityName_EntityId",
                schema: "audit",
                table: "AuditLogs",
                columns: new[] { "TenantId", "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_Timestamp",
                schema: "audit",
                table: "AuditLogs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_UserId",
                schema: "audit",
                table: "AuditLogs",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_TenantId_BrokerCode",
                schema: "master",
                table: "Brokers",
                columns: new[] { "TenantId", "BrokerCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Effectiv~",
                schema: "ledger",
                table: "ChargesConfigurations",
                columns: new[] { "TenantId", "BrokerId", "ChargeType", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_BrokerId",
                schema: "master",
                table: "Clients",
                column: "BrokerId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_BrokerId",
                schema: "master",
                table: "Clients",
                columns: new[] { "TenantId", "BrokerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_ClientCode",
                schema: "master",
                table: "Clients",
                columns: new[] { "TenantId", "ClientCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_TenantId_ExDate",
                schema: "corporate",
                table: "CorporateActions",
                columns: new[] { "TenantId", "ExDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_TenantId_InstrumentId_ActionType_ExDate",
                schema: "corporate",
                table: "CorporateActions",
                columns: new[] { "TenantId", "InstrumentId", "ActionType", "ExDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_TenantId_ExchangeCode",
                schema: "master",
                table: "Exchanges",
                columns: new[] { "TenantId", "ExchangeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_ExchangeId",
                schema: "master",
                table: "Instruments",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_ISIN",
                schema: "master",
                table: "Instruments",
                column: "ISIN");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_SegmentId",
                schema: "master",
                table: "Instruments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_TenantId_InstrumentCode",
                schema: "master",
                table: "Instruments",
                columns: new[] { "TenantId", "InstrumentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_TenantId_Symbol_ExchangeId",
                schema: "master",
                table: "Instruments",
                columns: new[] { "TenantId", "Symbol", "ExchangeId" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_TenantId_ClientId_PostingDate",
                schema: "ledger",
                table: "LedgerEntries",
                columns: new[] { "TenantId", "ClientId", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_TenantId_ReferenceType_ReferenceId",
                schema: "ledger",
                table: "LedgerEntries",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_VoucherNo",
                schema: "ledger",
                table: "LedgerEntries",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionCode",
                schema: "master",
                table: "Permissions",
                column: "PermissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PnLSnapshots_TenantId_ClientId_InstrumentId_SnapshotDate",
                schema: "trading",
                table: "PnLSnapshots",
                columns: new[] { "TenantId", "ClientId", "InstrumentId", "SnapshotDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PnLSnapshots_TenantId_SnapshotDate",
                schema: "trading",
                table: "PnLSnapshots",
                columns: new[] { "TenantId", "SnapshotDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_TenantId_ClientId_InstrumentId_PositionDate",
                schema: "trading",
                table: "Positions",
                columns: new[] { "TenantId", "ClientId", "InstrumentId", "PositionDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_TenantId_PositionDate",
                schema: "trading",
                table: "Positions",
                columns: new[] { "TenantId", "PositionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_TenantId_ReconDate",
                schema: "recon",
                table: "Reconciliations",
                columns: new[] { "TenantId", "ReconDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_TenantId_SettlementNo_ReconType",
                schema: "recon",
                table: "Reconciliations",
                columns: new[] { "TenantId", "SettlementNo", "ReconType" });

            migrationBuilder.CreateIndex(
                name: "IX_ReconExceptions_ReconId",
                schema: "recon",
                table: "ReconExceptions",
                column: "ReconId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconExceptions_TenantId_ReconId",
                schema: "recon",
                table: "ReconExceptions",
                columns: new[] { "TenantId", "ReconId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReconExceptions_TenantId_Status",
                schema: "recon",
                table: "ReconExceptions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "master",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                schema: "master",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_RoleName",
                schema: "master",
                table: "Roles",
                columns: new[] { "TenantId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Segments_ExchangeId",
                schema: "master",
                table: "Segments",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Segments_TenantId_ExchangeId_SegmentCode",
                schema: "master",
                table: "Segments",
                columns: new[] { "TenantId", "ExchangeId", "SegmentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatches_TenantId_SettlementNo",
                schema: "settlement",
                table: "SettlementBatches",
                columns: new[] { "TenantId", "SettlementNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatches_TenantId_TradeDate",
                schema: "settlement",
                table: "SettlementBatches",
                columns: new[] { "TenantId", "TradeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementObligations_BatchId",
                schema: "settlement",
                table: "SettlementObligations",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementObligations_TenantId_BatchId",
                schema: "settlement",
                table: "SettlementObligations",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementObligations_TenantId_SettlementNo_ClientId",
                schema: "settlement",
                table: "SettlementObligations",
                columns: new[] { "TenantId", "SettlementNo", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TenantCode",
                schema: "master",
                table: "Tenants",
                column: "TenantCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TenantId_ClientId_InstrumentId",
                schema: "trading",
                table: "Trades",
                columns: new[] { "TenantId", "ClientId", "InstrumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TenantId_TradeDate",
                schema: "trading",
                table: "Trades",
                columns: new[] { "TenantId", "TradeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TradeNo",
                schema: "trading",
                table: "Trades",
                column: "TradeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "master",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                schema: "master",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                schema: "master",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Username",
                schema: "master",
                table: "Users",
                columns: new[] { "TenantId", "Username" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ChargesConfigurations",
                schema: "ledger");

            migrationBuilder.DropTable(
                name: "Clients",
                schema: "master");

            migrationBuilder.DropTable(
                name: "CorporateActions",
                schema: "corporate");

            migrationBuilder.DropTable(
                name: "Instruments",
                schema: "master");

            migrationBuilder.DropTable(
                name: "LedgerEntries",
                schema: "ledger");

            migrationBuilder.DropTable(
                name: "PnLSnapshots",
                schema: "trading");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "trading");

            migrationBuilder.DropTable(
                name: "ReconExceptions",
                schema: "recon");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "master");

            migrationBuilder.DropTable(
                name: "SettlementObligations",
                schema: "settlement");

            migrationBuilder.DropTable(
                name: "Trades",
                schema: "trading");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Brokers",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Segments",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Reconciliations",
                schema: "recon");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "master");

            migrationBuilder.DropTable(
                name: "SettlementBatches",
                schema: "settlement");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Exchanges",
                schema: "master");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "master");
        }
    }
}
