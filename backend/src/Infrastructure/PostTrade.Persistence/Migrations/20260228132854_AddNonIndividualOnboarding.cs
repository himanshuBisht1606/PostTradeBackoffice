using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNonIndividualOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnnualTurnover",
                schema: "master",
                table: "Clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConstitutionType",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfConstitution",
                schema: "master",
                table: "Clients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityRegistrationNumber",
                schema: "master",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GSTNumber",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KartaName",
                schema: "master",
                table: "Clients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KartaPan",
                schema: "master",
                table: "Clients",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientAuthorizedSignatories",
                schema: "master",
                columns: table => new
                {
                    SignatoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Pan = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Din = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_ClientAuthorizedSignatories", x => x.SignatoryId);
                    table.ForeignKey(
                        name: "FK_ClientAuthorizedSignatories_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "master",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAuthorizedSignatories_ClientId",
                schema: "master",
                table: "ClientAuthorizedSignatories",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAuthorizedSignatories",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "AnnualTurnover",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ConstitutionType",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DateOfConstitution",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EntityRegistrationNumber",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "GSTNumber",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "KartaName",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "KartaPan",
                schema: "master",
                table: "Clients");
        }
    }
}
