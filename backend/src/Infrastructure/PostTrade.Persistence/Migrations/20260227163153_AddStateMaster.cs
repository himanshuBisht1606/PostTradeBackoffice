using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostTrade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStateMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reference");

            migrationBuilder.CreateTable(
                name: "StateMaster",
                schema: "reference",
                columns: table => new
                {
                    StateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    StateCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    StateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NseCode = table.Column<int>(type: "integer", nullable: true),
                    BseName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CvlCode = table.Column<int>(type: "integer", nullable: true),
                    NdmlCode = table.Column<int>(type: "integer", nullable: true),
                    NcdexCode = table.Column<int>(type: "integer", nullable: true),
                    NseKraCode = table.Column<int>(type: "integer", nullable: true),
                    NsdlCode = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_StateMaster", x => x.StateId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateMaster_StateCode",
                schema: "reference",
                table: "StateMaster",
                column: "StateCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateMaster",
                schema: "reference");
        }
    }
}
