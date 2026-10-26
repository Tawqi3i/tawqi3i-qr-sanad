using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class AddIdentityCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_ARABIC_NAME",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_BIRTH_DATE",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_BLOOD_TYPE",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_CARD_NUMBER",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_ENGLISH_NAME",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_EXPIRY_DATE",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_GENDER",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_MOTHER_NAME",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_NATIONAL_NUMBER",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_REGISTRY_NUMBER",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDENTITY_CARD_RESIDENCE",
                table: "SECURITY_AUDIT_LOGS",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SECURITY_AUDIT_LOGS_IDENTITY_CARD_NATIONAL_NUMBER",
                table: "SECURITY_AUDIT_LOGS",
                column: "IDENTITY_CARD_NATIONAL_NUMBER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SECURITY_AUDIT_LOGS_IDENTITY_CARD_NATIONAL_NUMBER",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_ARABIC_NAME",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_BIRTH_DATE",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_BLOOD_TYPE",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_CARD_NUMBER",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_ENGLISH_NAME",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_EXPIRY_DATE",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_GENDER",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_MOTHER_NAME",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_NATIONAL_NUMBER",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_REGISTRY_NUMBER",
                table: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "IDENTITY_CARD_RESIDENCE",
                table: "SECURITY_AUDIT_LOGS");
        }
    }
}
