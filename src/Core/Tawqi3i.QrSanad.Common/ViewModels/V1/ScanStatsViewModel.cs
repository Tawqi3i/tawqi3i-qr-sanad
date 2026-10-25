namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

public class ScanCountsViewModel
{
    public int Succeeded { get; set; }

    public int Failed { get; set; }

    public int Total => this.Succeeded + this.Failed;
}

public class ScanStatsViewModel : ScanCountsViewModel
{
    /// <summary>
    /// The range covered, UTC (from inclusive, to exclusive).
    /// </summary>
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    /// <summary>
    /// One entry per calendar day of the viewer, oldest first, including days without scans.
    /// </summary>
    public List<DailyScanCountsViewModel> Daily { get; set; } = [];

    /// <summary>
    /// The users with the most scans, most first.
    /// </summary>
    public List<UserScanCountsViewModel> TopUsers { get; set; } = [];

    /// <summary>
    /// Per branch, most scans first. Only for the SuperAdmin; null for everyone else.
    /// </summary>
    public List<BranchScanCountsViewModel>? Branches { get; set; }
}

public class DailyScanCountsViewModel : ScanCountsViewModel
{
    public DateOnly Date { get; set; }
}

public class UserScanCountsViewModel : ScanCountsViewModel
{
    public Guid UserId { get; set; }

    public string? UserName { get; set; }
}

public class BranchScanCountsViewModel : ScanCountsViewModel
{
    public Guid BranchId { get; set; }

    /// <summary>
    /// Null when the branch has since been deleted.
    /// </summary>
    public string? BranchName { get; set; }
}
