using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tawqi3i.QrSanad.Persistence.Migrations.MySql
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    code = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "security_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    event_type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    user_name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    branch_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    ip_address = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_security_audit_logs", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    normalized_name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    address = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    phone_number = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    admin_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branches", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    normalized_user_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    normalized_email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    user_type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    branch_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    password_hash = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    security_stamp = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    concurrency_stamp = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    access_failed_count = table.Column<int>(type: "int", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    password_changed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    must_change_password = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    security_stamp = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_ip = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    revoke_reason = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    permission_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    assigned_by = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_permissions", x => new { x.user_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_user_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description" },
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
                name: "ix_branches_admin_user_id",
                table: "branches",
                column: "admin_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_code",
                table: "branches",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_normalized_name",
                table: "branches",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_revoked_at",
                table: "refresh_tokens",
                columns: new[] { "user_id", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_security_audit_logs_branch_id_created_at",
                table: "security_audit_logs",
                columns: new[] { "branch_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_security_audit_logs_created_at",
                table: "security_audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_security_audit_logs_user_id_created_at",
                table: "security_audit_logs",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_permissions_permission_id",
                table: "user_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_branch_id_user_type",
                table: "users",
                columns: new[] { "branch_id", "user_type" });

            migrationBuilder.CreateIndex(
                name: "ix_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_user_name",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_branches_users_admin_user_id",
                table: "branches",
                column: "admin_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_branches_users_admin_user_id",
                table: "branches");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "security_audit_logs");

            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "branches");
        }
    }
}
