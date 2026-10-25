using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddESealPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description" },
                values: new object[] { new Guid("8ac78524-d7b8-861f-9978-2a67584e06a8"), "eseal.view", "Retrieve e-sealed documents by SANAD QR code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8ac78524-d7b8-861f-9978-2a67584e06a8"));
        }
    }
}
