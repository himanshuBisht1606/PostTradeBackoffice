using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Exchanges_ExchangeId",
                schema: "master",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Segments_ExchangeId",
                schema: "master",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Segments_TenantId_ExchangeId_SegmentCode",
                schema: "master",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "ExchangeId",
                schema: "master",
                table: "Segments");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "master",
                table: "Segments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aadhaar",
                schema: "master",
                table: "Clients",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "master",
                table: "Clients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DematAccountNo",
                schema: "master",
                table: "Clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Depository",
                schema: "master",
                table: "Clients",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KYCStatus",
                schema: "master",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskCategory",
                schema: "master",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StateCode",
                schema: "master",
                table: "Clients",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateName",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "master",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BranchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    StateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GSTIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeSegments",
                schema: "master",
                columns: table => new
                {
                    ExchangeSegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeSegmentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExchangeSegmentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SettlementType = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ExchangeSegments", x => x.ExchangeSegmentId);
                    table.ForeignKey(
                        name: "FK_ExchangeSegments_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalSchema: "master",
                        principalTable: "Exchanges",
                        principalColumn: "ExchangeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeSegments_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalSchema: "master",
                        principalTable: "Segments",
                        principalColumn: "SegmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeSegments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientSegmentActivations",
                schema: "master",
                columns: table => new
                {
                    ActivationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeSegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExposureLimit = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    MarginType = table.Column<int>(type: "integer", nullable: false),
                    ActivatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeactivatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ClientSegmentActivations", x => x.ActivationId);
                    table.ForeignKey(
                        name: "FK_ClientSegmentActivations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "master",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientSegmentActivations_ExchangeSegments_ExchangeSegmentId",
                        column: x => x.ExchangeSegmentId,
                        principalSchema: "master",
                        principalTable: "ExchangeSegments",
                        principalColumn: "ExchangeSegmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientSegmentActivations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "master",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Segments_TenantId_SegmentCode",
                schema: "master",
                table: "Segments",
                columns: new[] { "TenantId", "SegmentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_BranchId",
                schema: "master",
                table: "Clients",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_BranchCode",
                schema: "master",
                table: "Branches",
                columns: new[] { "TenantId", "BranchCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientSegmentActivations_ClientId",
                schema: "master",
                table: "ClientSegmentActivations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSegmentActivations_ExchangeSegmentId",
                schema: "master",
                table: "ClientSegmentActivations",
                column: "ExchangeSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSegmentActivations_TenantId_ClientId_ExchangeSegmentId",
                schema: "master",
                table: "ClientSegmentActivations",
                columns: new[] { "TenantId", "ClientId", "ExchangeSegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeSegments_ExchangeId",
                schema: "master",
                table: "ExchangeSegments",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeSegments_SegmentId",
                schema: "master",
                table: "ExchangeSegments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeSegments_TenantId_ExchangeId_ExchangeSegmentCode",
                schema: "master",
                table: "ExchangeSegments",
                columns: new[] { "TenantId", "ExchangeId", "ExchangeSegmentCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Branches_BranchId",
                schema: "master",
                table: "Clients",
                column: "BranchId",
                principalSchema: "master",
                principalTable: "Branches",
                principalColumn: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Branches_BranchId",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ClientSegmentActivations",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ExchangeSegments",
                schema: "master");

            migrationBuilder.DropIndex(
                name: "IX_Segments_TenantId_SegmentCode",
                schema: "master",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Clients_BranchId",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "master",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "Aadhaar",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DematAccountNo",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Depository",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "KYCStatus",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RiskCategory",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "StateCode",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "StateName",
                schema: "master",
                table: "Clients");

            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeId",
                schema: "master",
                table: "Segments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Exchanges_ExchangeId",
                schema: "master",
                table: "Segments",
                column: "ExchangeId",
                principalSchema: "master",
                principalTable: "Exchanges",
                principalColumn: "ExchangeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
