using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChargesConfigSegmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Segment",
                schema: "ledger",
                table: "ChargesConfigurations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "All");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableTo",
                schema: "ledger",
                table: "ChargesConfigurations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Both");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                schema: "ledger",
                table: "ChargesConfigurations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_EffectiveFrom"";
DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Effective"";");

            migrationBuilder.CreateIndex(
                name: "IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Segment_EffectiveFrom",
                schema: "ledger",
                table: "ChargesConfigurations",
                columns: new[] { "TenantId", "BrokerId", "ChargeType", "Segment", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Segment_EffectiveFrom",
                schema: "ledger",
                table: "ChargesConfigurations");

            migrationBuilder.DropColumn(
                name: "Segment",
                schema: "ledger",
                table: "ChargesConfigurations");

            migrationBuilder.DropColumn(
                name: "ApplicableTo",
                schema: "ledger",
                table: "ChargesConfigurations");

            migrationBuilder.DropColumn(
                name: "Remarks",
                schema: "ledger",
                table: "ChargesConfigurations");

            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ledger.""IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_Segment_EffectiveFrom"";");
            migrationBuilder.CreateIndex(
                name: "IX_ChargesConfigurations_TenantId_BrokerId_ChargeType_EffectiveFrom",
                schema: "ledger",
                table: "ChargesConfigurations",
                columns: new[] { "TenantId", "BrokerId", "ChargeType", "EffectiveFrom" });
        }
    }
}
