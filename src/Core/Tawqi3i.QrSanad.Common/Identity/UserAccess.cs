namespace Tawqi3i.QrSanad.Common.Identity;

using Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// A cached snapshot of a user's security state, consulted on every authenticated request.
/// </summary>
public sealed class UserAccess
{
    public Guid UserId { get; init; }

    public UserType UserType { get; init; }

    public Guid? BranchId { get; init; }

    public string SecurityStamp { get; init; } = string.Empty;

    /// <summary>
    /// False when the user is disabled or their branch is disabled or deleted.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Set when an admin reset the password or the password policy changed. Checked live, so it
    /// applies to access tokens issued before the flag was set.
    /// </summary>
    public bool MustChangePassword { get; init; }

    /// <summary>
    /// For the SuperAdmin, every code; for a device, none.
    /// </summary>
    public string[] Permissions { get; init; } = [];

    public bool IsSuperAdmin => this.UserType == UserType.SuperAdmin;

    public bool IsDevice => this.UserType == UserType.Device;

    public bool HasPermission(string code) => this.IsSuperAdmin || this.Permissions.Contains(code, StringComparer.Ordinal);
}
