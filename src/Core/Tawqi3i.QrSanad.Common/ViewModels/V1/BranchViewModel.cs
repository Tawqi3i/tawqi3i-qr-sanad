namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

using Tawqi3i.QrSanad.Common.Enums;

public class BranchViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public BranchStatus Status { get; set; }

    /// <summary>
    /// Null until an admin has been created for the branch.
    /// </summary>
    public UserSummaryViewModel? Admin { get; set; }

    public int DevicesCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
