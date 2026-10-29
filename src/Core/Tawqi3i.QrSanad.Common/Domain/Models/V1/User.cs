namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

using Tawqi3i.QrSanad.Common.Enums;

public class User : AuditableEntity
{
    public string UserName { get; set; } = string.Empty;

    public string NormalizedUserName { get; set; } = string.Empty;

    /// <summary>
    /// Required for admins and the SuperAdmin; devices have none.
    /// </summary>
    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserType UserType { get; set; }

    /// <summary>
    /// Required for admins and devices; null only for the SuperAdmin.
    /// </summary>
    public Guid? BranchId { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Rotated whenever existing sessions must end. Access and refresh tokens carry it and are
    /// rejected once it changes.
    /// </summary>
    public string SecurityStamp { get; set; } = string.Empty;

    /// <summary>
    /// Optimistic concurrency token, regenerated on every administrative change.
    /// </summary>
    public string ConcurrencyStamp { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public int AccessFailedCount { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? PasswordChangedAt { get; set; }

    public bool MustChangePassword { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public Branch? Branch { get; set; }

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
