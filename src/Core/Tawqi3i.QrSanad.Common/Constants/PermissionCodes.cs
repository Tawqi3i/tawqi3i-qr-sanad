namespace Tawqi3i.QrSanad.Common.Constants;

/// <summary>
/// The permission catalogue. Every code is seeded into the Permissions table by the migrations
/// (see SeedsFactory) and checked on endpoints with [HasPermission].
///
/// The SuperAdmin holds all of them implicitly. A new admin receives all of them unless the
/// SuperAdmin chooses otherwise, and can pass on to their branch's users only what they hold.
///
/// Adding a code means adding a migration for each provider so the row exists.
/// </summary>
public static class PermissionCodes
{
    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string ResetPassword = "users.reset_password";
        public const string AssignPermissions = "users.assign_permissions";
    }

    public static class Permissions
    {
        public const string View = "permissions.view";
    }

    public static class AuditLogs
    {
        public const string View = "audit_logs.view";
    }

    public static class ESeal
    {
        public const string View = "eseal.view";
    }

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(Users.View, "View the users of the branch"),
        new(Users.Create, "Create users in the branch"),
        new(Users.Update, "Update users, change their status and unlock them"),
        new(Users.Delete, "Delete users"),
        new(Users.ResetPassword, "Reset another user's password"),
        new(Users.AssignPermissions, "Grant and revoke user permissions"),

        new(Permissions.View, "View the permission catalogue"),

        new(AuditLogs.View, "View the security audit log of the branch"),

        new(ESeal.View, "Retrieve e-sealed documents by SANAD QR code"),
    ];

    public static IReadOnlyList<string> AllCodes { get; } = All.Select(a => a.Code).ToList();
}

public sealed record PermissionDefinition(string Code, string Description);
