namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

using Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// The signed-in user, with everything a client needs to decide what to show.
/// </summary>
public class ProfileViewModel
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserType Type { get; set; }

    public Guid? BranchId { get; set; }

    public string? BranchName { get; set; }

    public bool MustChangePassword { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? PasswordChangedAt { get; set; }

    public ICollection<string> Permissions { get; set; } = [];
}
