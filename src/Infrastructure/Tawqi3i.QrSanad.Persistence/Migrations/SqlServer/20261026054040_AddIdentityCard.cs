using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddIdentityCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_ArabicName",
                table: "SecurityAuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "IdentityCard_BirthDate",
                table: "SecurityAuditLogs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_BloodType",
                table: "SecurityAuditLogs",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_CardNumber",
                table: "SecurityAuditLogs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_EnglishName",
                table: "SecurityAuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "IdentityCard_ExpiryDate",
                table: "SecurityAuditLogs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_Gender",
                table: "SecurityAuditLogs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_MotherName",
                table: "SecurityAuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_NationalNumber",
                table: "SecurityAuditLogs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_RegistryNumber",
                table: "SecurityAuditLogs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard_Residence",
                table: "SecurityAuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_IdentityCard_NationalNumber",
                table: "SecurityAuditLogs",
                column: "IdentityCard_NationalNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SecurityAuditLogs_IdentityCard_NationalNumber",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_ArabicName",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_BirthDate",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_BloodType",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_CardNumber",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_EnglishName",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_ExpiryDate",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_Gender",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_MotherName",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_NationalNumber",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_RegistryNumber",
                table: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdentityCard_Residence",
                table: "SecurityAuditLogs");
        }
    }
}
