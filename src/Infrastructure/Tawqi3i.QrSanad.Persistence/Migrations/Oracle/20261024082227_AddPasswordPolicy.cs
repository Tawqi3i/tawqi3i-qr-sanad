using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class AddPasswordPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PASSWORD_POLICIES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    MIN_LENGTH = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MAX_LENGTH = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REQUIRE_UPPERCASE = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    REQUIRE_LOWERCASE = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    REQUIRE_DIGIT = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    REQUIRE_NON_ALPHANUMERIC = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CREATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UPDATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASSWORD_POLICIES", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PASSWORD_POLICIES");
        }
    }
}
