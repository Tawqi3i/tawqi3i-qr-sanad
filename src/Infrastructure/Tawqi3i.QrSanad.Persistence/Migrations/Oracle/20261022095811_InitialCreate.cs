using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tawqi3i.QrSanad.Persistence.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PERMISSIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSIONS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SECURITY_AUDIT_LOGS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EVENT_TYPE = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    USER_NAME = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    ACTOR_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    BRANCH_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    IP_ADDRESS = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    USER_AGENT = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: true),
                    DETAILS = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SECURITY_AUDIT_LOGS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BRANCHES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    NORMALIZED_NAME = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: true),
                    ADDRESS = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    ADMIN_USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CONCURRENCY_STAMP = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    IS_DELETED = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DELETED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DELETED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CREATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UPDATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BRANCHES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_NAME = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    NORMALIZED_USER_NAME = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    NORMALIZED_EMAIL = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    DISPLAY_NAME = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    PHONE_NUMBER = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: true),
                    USER_TYPE = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    BRANCH_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: false),
                    SECURITY_STAMP = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    ACCESS_FAILED_COUNT = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LOCKOUT_END = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    LAST_LOGIN_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    PASSWORD_CHANGED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    MUST_CHANGE_PASSWORD = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DELETED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DELETED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CREATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UPDATED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USERS_BRANCHES_BRANCH_ID",
                        column: x => x.BRANCH_ID,
                        principalTable: "BRANCHES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REFRESH_TOKENS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TOKEN_HASH = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    SECURITY_STAMP = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EXPIRES_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CREATED_BY_IP = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    USER_AGENT = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: true),
                    REVOKED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    REVOKED_BY_IP = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    REVOKE_REASON = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    REPLACED_BY_TOKEN_ID = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESH_TOKENS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REFRESH_TOKENS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_PERMISSIONS",
                columns: table => new
                {
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PERMISSION_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ASSIGNED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ASSIGNED_BY = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_PERMISSIONS", x => new { x.USER_ID, x.PERMISSION_ID });
                    table.ForeignKey(
                        name: "FK_USER_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                        column: x => x.PERMISSION_ID,
                        principalTable: "PERMISSIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_PERMISSIONS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PERMISSIONS",
                columns: new[] { "ID", "CODE", "DESCRIPTION" },
                values: new object[,]
                {
                    { new Guid("6b5dd7de-fed2-8a5b-8064-b4d9a2551f9d"), "users.update", "Update users, change their status and unlock them" },
                    { new Guid("935d225d-1b6c-830d-916b-c94acbf35197"), "users.reset_password", "Reset another user's password" },
                    { new Guid("9b5270e0-5623-8d12-bf45-ae8cd430b855"), "users.assign_permissions", "Grant and revoke user permissions" },
                    { new Guid("a1885bbe-d02b-8363-bf99-25e9c4bad98d"), "users.create", "Create users in the branch" },
                    { new Guid("b5b3840b-f1b5-8912-909e-80df84a42698"), "users.delete", "Delete users" },
                    { new Guid("c7994376-f8e1-8e70-a2f6-5de3a6ea4963"), "permissions.view", "View the permission catalogue" },
                    { new Guid("f969d5a8-1f29-8efb-b321-4e7cc4654082"), "audit_logs.view", "View the security audit log of the branch" },
                    { new Guid("faebb80f-3f91-88d3-8344-93229a4b7798"), "users.view", "View the users of the branch" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BRANCHES_ADMIN_USER_ID",
                table: "BRANCHES",
                column: "ADMIN_USER_ID",
                unique: true,
                filter: "\"ADMIN_USER_ID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BRANCHES_CODE",
                table: "BRANCHES",
                column: "CODE",
                unique: true,
                filter: "\"CODE\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BRANCHES_NORMALIZED_NAME",
                table: "BRANCHES",
                column: "NORMALIZED_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PERMISSIONS_CODE",
                table: "PERMISSIONS",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_TOKEN_HASH",
                table: "REFRESH_TOKENS",
                column: "TOKEN_HASH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_USER_ID_REVOKED_AT",
                table: "REFRESH_TOKENS",
                columns: new[] { "USER_ID", "REVOKED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_SECURITY_AUDIT_LOGS_BRANCH_ID_CREATED_AT",
                table: "SECURITY_AUDIT_LOGS",
                columns: new[] { "BRANCH_ID", "CREATED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_SECURITY_AUDIT_LOGS_CREATED_AT",
                table: "SECURITY_AUDIT_LOGS",
                column: "CREATED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_SECURITY_AUDIT_LOGS_USER_ID_CREATED_AT",
                table: "SECURITY_AUDIT_LOGS",
                columns: new[] { "USER_ID", "CREATED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_USER_PERMISSIONS_PERMISSION_ID",
                table: "USER_PERMISSIONS",
                column: "PERMISSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_BRANCH_ID_USER_TYPE",
                table: "USERS",
                columns: new[] { "BRANCH_ID", "USER_TYPE" });

            migrationBuilder.CreateIndex(
                name: "IX_USERS_CREATED_AT",
                table: "USERS",
                column: "CREATED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_NORMALIZED_EMAIL",
                table: "USERS",
                column: "NORMALIZED_EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_NORMALIZED_USER_NAME",
                table: "USERS",
                column: "NORMALIZED_USER_NAME",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BRANCHES_USERS_ADMIN_USER_ID",
                table: "BRANCHES",
                column: "ADMIN_USER_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BRANCHES_USERS_ADMIN_USER_ID",
                table: "BRANCHES");

            migrationBuilder.DropTable(
                name: "REFRESH_TOKENS");

            migrationBuilder.DropTable(
                name: "SECURITY_AUDIT_LOGS");

            migrationBuilder.DropTable(
                name: "USER_PERMISSIONS");

            migrationBuilder.DropTable(
                name: "PERMISSIONS");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "BRANCHES");
        }
    }
}
