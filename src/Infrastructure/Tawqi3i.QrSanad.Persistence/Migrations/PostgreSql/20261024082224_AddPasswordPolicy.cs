using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.PostgreSql
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_length = table.Column<int>(type: "integer", nullable: false),
                    max_length = table.Column<int>(type: "integer", nullable: false),
                    require_uppercase = table.Column<bool>(type: "boolean", nullable: false),
                    require_lowercase = table.Column<bool>(type: "boolean", nullable: false),
                    require_digit = table.Column<bool>(type: "boolean", nullable: false),
                    require_non_alphanumeric = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_policies", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_policies");
        }
    }
}
