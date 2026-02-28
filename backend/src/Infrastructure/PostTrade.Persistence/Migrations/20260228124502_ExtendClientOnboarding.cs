using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendClientOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipStatus",
                schema: "master",
                table: "Clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolderType",
                schema: "master",
                table: "Clients",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Single");

            migrationBuilder.AddColumn<string>(
                name: "IdentityProofNumber",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityProofType",
                schema: "master",
                table: "Clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentialStatus",
                schema: "master",
                table: "Clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SegmentBSE",
                schema: "master",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SegmentMCX",
                schema: "master",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SegmentNSE",
                schema: "master",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClientFatca",
                schema: "master",
                columns: table => new
                {
                    ClientFatcaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsUsPerson = table.Column<bool>(type: "boolean", nullable: false),
                    SourceOfWealth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_ClientFatca", x => x.ClientFatcaId);
                    table.ForeignKey(
                        name: "FK_ClientFatca_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "master",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientJointHolders",
                schema: "master",
                columns: table => new
                {
                    JointHolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HolderNumber = table.Column<int>(type: "integer", nullable: false),
                    Pan = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_ClientJointHolders", x => x.JointHolderId);
                    table.ForeignKey(
                        name: "FK_ClientJointHolders_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "master",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientFatca_ClientId",
                schema: "master",
                table: "ClientFatca",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientJointHolders_ClientId_HolderNumber",
                schema: "master",
                table: "ClientJointHolders",
                columns: new[] { "ClientId", "HolderNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientFatca",
                schema: "master");

            migrationBuilder.DropTable(
                name: "ClientJointHolders",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "AccountType",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BranchName",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CitizenshipStatus",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "HolderType",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IdentityProofNumber",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IdentityProofType",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ResidentialStatus",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SegmentBSE",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SegmentMCX",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SegmentNSE",
                schema: "master",
                table: "Clients");
        }
    }
}
