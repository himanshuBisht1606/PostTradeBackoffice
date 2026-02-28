using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAndDpMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankMapping",
                schema: "reference",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IFSCCode = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    MICRCode = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
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
                    table.PrimaryKey("PK_BankMapping", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "BankMaster",
                schema: "reference",
                columns: table => new
                {
                    BankId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IFSCPrefix = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
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
                    table.PrimaryKey("PK_BankMaster", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "CdslDpMaster",
                schema: "reference",
                columns: table => new
                {
                    DpId = table.Column<Guid>(type: "uuid", nullable: false),
                    DpCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DpName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SebiRegNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MemberStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_CdslDpMaster", x => x.DpId);
                });

            migrationBuilder.CreateTable(
                name: "NsdlDpMaster",
                schema: "reference",
                columns: table => new
                {
                    DpId = table.Column<Guid>(type: "uuid", nullable: false),
                    DpCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DpName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SebiRegNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MemberStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_NsdlDpMaster", x => x.DpId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankMapping_IFSCCode",
                schema: "reference",
                table: "BankMapping",
                column: "IFSCCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankMaster_BankCode",
                schema: "reference",
                table: "BankMaster",
                column: "BankCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CdslDpMaster_DpCode",
                schema: "reference",
                table: "CdslDpMaster",
                column: "DpCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NsdlDpMaster_DpCode",
                schema: "reference",
                table: "NsdlDpMaster",
                column: "DpCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankMapping",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "BankMaster",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "CdslDpMaster",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "NsdlDpMaster",
                schema: "reference");
        }
    }
}
