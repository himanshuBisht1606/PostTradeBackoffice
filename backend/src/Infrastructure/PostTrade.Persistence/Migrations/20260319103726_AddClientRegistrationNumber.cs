using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientRegistrationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_ClientCode",
                schema: "master",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "ClientCode",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Back-fill existing clients with unique registration numbers (REG-000001 … per tenant)
            migrationBuilder.Sql(@"
                UPDATE master.""Clients"" c
                SET ""RegistrationNumber"" = 'REG-' || LPAD(rn::text, 6, '0')
                FROM (
                    SELECT ""ClientId"",
                           ROW_NUMBER() OVER (PARTITION BY ""TenantId"" ORDER BY ""CreatedAt"") AS rn
                    FROM master.""Clients""
                ) ranked
                WHERE c.""ClientId"" = ranked.""ClientId"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_ClientCode",
                schema: "master",
                table: "Clients",
                columns: new[] { "TenantId", "ClientCode" },
                unique: true,
                filter: "\"ClientCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_RegistrationNumber",
                schema: "master",
                table: "Clients",
                columns: new[] { "TenantId", "RegistrationNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_ClientCode",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_RegistrationNumber",
                schema: "master",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                schema: "master",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "ClientCode",
                schema: "master",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_ClientCode",
                schema: "master",
                table: "Clients",
                columns: new[] { "TenantId", "ClientCode" },
                unique: true);
        }
    }
}
