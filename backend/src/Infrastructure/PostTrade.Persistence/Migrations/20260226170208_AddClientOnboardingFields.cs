using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternateMobile",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrespondenceAddress",
                schema: "master",
                table: "Clients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "master",
                table: "Clients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherSpouseName",
                schema: "master",
                table: "Clients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrossAnnualIncome",
                schema: "master",
                table: "Clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                schema: "master",
                table: "Clients",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                schema: "master",
                table: "Clients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PinCode",
                schema: "master",
                table: "Clients",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientNominees",
                schema: "master",
                columns: table => new
                {
                    NomineeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomineeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    NomineePAN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SharePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ClientNominees", x => x.NomineeId);
                    table.ForeignKey(
                        name: "FK_ClientNominees_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "master",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientNominees_ClientId",
                schema: "master",
                table: "ClientNominees",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientNominees",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "AlternateMobile",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CorrespondenceAddress",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FatherSpouseName",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "GrossAnnualIncome",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MotherName",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Occupation",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PinCode",
                schema: "master",
                table: "Clients");
        }
    }
}
