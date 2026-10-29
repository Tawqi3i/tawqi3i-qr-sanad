using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class LicenseConcurrencyStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Licenses",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                // Not empty: Oracle stores '' as NULL, which the NOT NULL column rejects.
                defaultValue: "initial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Licenses");
        }
    }
}
