using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class AddESealPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PERMISSIONS",
                columns: new[] { "ID", "CODE", "DESCRIPTION" },
                values: new object[] { new Guid("8ac78524-d7b8-861f-9978-2a67584e06a8"), "eseal.view", "Retrieve e-sealed documents by SANAD QR code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PERMISSIONS",
                keyColumn: "ID",
                keyValue: new Guid("8ac78524-d7b8-861f-9978-2a67584e06a8"));
        }
    }
}
