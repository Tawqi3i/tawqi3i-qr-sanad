namespace Tawqi3i.QrSanad.Common.Constants;

/// <summary>
/// The permission catalogue. Every code is seeded into the Permissions table by the migrations
/// (see SeedsFactory) and checked on endpoints with [HasPermission].
///
/// Permissions are held by admins only. The SuperAdmin holds all of them implicitly; a new admin
/// receives all of them unless the SuperAdmin chooses otherwise. Devices hold none: scanning is
/// decided by the user type, not by a permission.
///
/// Adding a code means adding a migration for each provider so the row exists.
/// </summary>
public static class PermissionCodes
{
    public static class Devices
    {
        public const string View = "devices.view";
        public const string Create = "devices.create";
        public const string Update = "devices.update";
        public const string Delete = "devices.delete";
        public const string ResetPassword = "devices.reset_password";
    }

    public static class AuditLogs
    {
        public const string View = "audit_logs.view";
    }

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(Devices.View, "View the devices of the branch"),
        new(Devices.Create, "Create devices in the branch"),
        new(Devices.Update, "Update devices, change their status and unlock them"),
        new(Devices.Delete, "Delete devices"),
        new(Devices.ResetPassword, "Reset a device's password"),
        new(AuditLogs.View, "View the security audit log and the scans of the branch"),
    ];

    public static IReadOnlyList<string> AllCodes { get; } = All.Select(a => a.Code).ToList();
}

public sealed record PermissionDefinition(string Code, string Description);
