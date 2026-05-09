using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichBrokerDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "master",
                table: "Brokers");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "master",
                table: "Brokers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PAN",
                schema: "master",
                table: "Brokers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GST",
                schema: "master",
                table: "Brokers",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CIN",
                schema: "master",
                table: "Brokers",
                type: "character varying(21)",
                maxLength: 21,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceOfficerEmail",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceOfficerName",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceOfficerPhone",
                schema: "master",
                table: "Brokers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrespondenceAddressLine1",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrespondenceAddressLine2",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrespondenceCity",
                schema: "master",
                table: "Brokers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrespondencePinCode",
                schema: "master",
                table: "Brokers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CorrespondenceSameAsRegistered",
                schema: "master",
                table: "Brokers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CorrespondenceState",
                schema: "master",
                table: "Brokers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityType",
                schema: "master",
                table: "Brokers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "IncorporationDate",
                schema: "master",
                table: "Brokers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrincipalOfficerEmail",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrincipalOfficerName",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrincipalOfficerPhone",
                schema: "master",
                table: "Brokers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredAddressLine1",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredAddressLine2",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredCity",
                schema: "master",
                table: "Brokers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredCountry",
                schema: "master",
                table: "Brokers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "India");

            migrationBuilder.AddColumn<string>(
                name: "RegisteredPinCode",
                schema: "master",
                table: "Brokers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredState",
                schema: "master",
                table: "Brokers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SEBIRegistrationDate",
                schema: "master",
                table: "Brokers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SEBIRegistrationExpiry",
                schema: "master",
                table: "Brokers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementBankAccountNo",
                schema: "master",
                table: "Brokers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementBankBranch",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementBankIfsc",
                schema: "master",
                table: "Brokers",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementBankName",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TAN",
                schema: "master",
                table: "Brokers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "master",
                table: "Brokers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BrokerExchangeMemberships",
                schema: "master",
                columns: table => new
                {
                    BrokerExchangeMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeSegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClearingMemberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MembershipType = table.Column<int>(type: "integer", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_BrokerExchangeMemberships", x => x.BrokerExchangeMembershipId);
                    table.ForeignKey(
                        name: "FK_BrokerExchangeMemberships_Brokers_BrokerId",
                        column: x => x.BrokerId,
                        principalSchema: "master",
                        principalTable: "Brokers",
                        principalColumn: "BrokerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrokerExchangeMemberships_ExchangeSegments_ExchangeSegmentId",
                        column: x => x.ExchangeSegmentId,
                        principalSchema: "master",
                        principalTable: "ExchangeSegments",
                        principalColumn: "ExchangeSegmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerExchangeMemberships_BrokerId_ExchangeSegmentId",
                schema: "master",
                table: "BrokerExchangeMemberships",
                columns: new[] { "BrokerId", "ExchangeSegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrokerExchangeMemberships_ExchangeSegmentId",
                schema: "master",
                table: "BrokerExchangeMemberships",
                column: "ExchangeSegmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrokerExchangeMemberships",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "CIN",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "ComplianceOfficerEmail",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "ComplianceOfficerName",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "ComplianceOfficerPhone",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "CorrespondenceAddressLine1",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "CorrespondenceAddressLine2",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "CorrespondenceCity",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "CorrespondencePinCode",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "CorrespondenceSameAsRegistered",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "CorrespondenceState",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "EntityType",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "IncorporationDate",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "PrincipalOfficerEmail",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "PrincipalOfficerName",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "PrincipalOfficerPhone",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "RegisteredAddressLine1",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "RegisteredAddressLine2",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "RegisteredCity",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "RegisteredCountry",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "RegisteredPinCode",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "RegisteredState",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "SEBIRegistrationDate",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "SEBIRegistrationExpiry",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "SettlementBankAccountNo",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "SettlementBankBranch",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "SettlementBankIfsc",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "SettlementBankName",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "TAN",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "master",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "master",
                table: "Brokers");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "master",
                table: "Brokers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PAN",
                schema: "master",
                table: "Brokers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GST",
                schema: "master",
                table: "Brokers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
