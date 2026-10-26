using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawqi3i.QrSanad.Persistence.Migrations.MySql
{
    /// <inheritdoc />
    public partial class AddIdentityCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_card_arabic_name",
                table: "security_audit_logs",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "identity_card_birth_date",
                table: "security_audit_logs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_blood_type",
                table: "security_audit_logs",
                type: "varchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_card_number",
                table: "security_audit_logs",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_english_name",
                table: "security_audit_logs",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "identity_card_expiry_date",
                table: "security_audit_logs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_gender",
                table: "security_audit_logs",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_mother_name",
                table: "security_audit_logs",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_national_number",
                table: "security_audit_logs",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_registry_number",
                table: "security_audit_logs",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_card_residence",
                table: "security_audit_logs",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_security_audit_logs_identity_card_national_number",
                table: "security_audit_logs",
                column: "identity_card_national_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_security_audit_logs_identity_card_national_number",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_arabic_name",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_birth_date",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_blood_type",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_card_number",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_english_name",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_expiry_date",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_gender",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_mother_name",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_national_number",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_registry_number",
                table: "security_audit_logs");

            migrationBuilder.DropColumn(
                name: "identity_card_residence",
                table: "security_audit_logs");
        }
    }
}
