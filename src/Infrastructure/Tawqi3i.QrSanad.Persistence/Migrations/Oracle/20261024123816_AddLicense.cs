using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class AddLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LICENSES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    KEY = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CREATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UPDATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LICENSES", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LICENSES");
        }
    }
}
