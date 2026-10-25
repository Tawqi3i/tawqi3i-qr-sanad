namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

public class LicenseViewModel
{
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// TRIAL, PROD or DEV.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired { get; set; }

    /// <summary>
    /// Whole days left, 0 once expired.
    /// </summary>
    public int DaysRemaining { get; set; }

    /// <summary>
    /// Admins and users allowed, not counting the SuperAdmin. Null means unlimited.
    /// </summary>
    public int? MaxUsers { get; set; }

    public int UsersCount { get; set; }

    /// <summary>
    /// Null means unlimited.
    /// </summary>
    public int? MaxBranches { get; set; }

    public int BranchesCount { get; set; }
}
