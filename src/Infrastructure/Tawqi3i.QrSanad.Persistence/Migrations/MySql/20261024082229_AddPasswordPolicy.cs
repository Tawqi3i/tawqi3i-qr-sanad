using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.MySql
{
    /// <inheritdoc />
    public partial class AddPasswordPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "password_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    min_length = table.Column<int>(type: "int", nullable: false),
                    max_length = table.Column<int>(type: "int", nullable: false),
                    require_uppercase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    require_lowercase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    require_digit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    require_non_alphanumeric = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_policies", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_policies");
        }
    }
}
