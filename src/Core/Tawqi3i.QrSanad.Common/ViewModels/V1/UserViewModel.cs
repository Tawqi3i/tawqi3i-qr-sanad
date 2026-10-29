namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

using Tawqi3i.QrSanad.Common.Enums;

public class UserViewModel
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Null for devices.
    /// </summary>
    public string? Email { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserType Type { get; set; }

    public Guid? BranchId { get; set; }

    public string? BranchName { get; set; }

    public UserStatus Status { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool MustChangePassword { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class UserDetailsViewModel : UserViewModel
{
    /// <summary>
    /// Everything the user can do. For the SuperAdmin, every code; for a device, none.
    /// </summary>
    public ICollection<string> Permissions { get; set; } = [];
}

public class UserSummaryViewModel
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserStatus Status { get; set; }
}
