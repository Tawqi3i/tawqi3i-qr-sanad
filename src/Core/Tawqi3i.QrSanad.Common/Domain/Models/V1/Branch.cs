namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

using Tawqi3i.QrSanad.Common.Enums;

public class Branch : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Optional short code, unique when set.
    /// </summary>
    public string? Code { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public BranchStatus Status { get; set; } = BranchStatus.Active;

    /// <summary>
    /// The branch's one admin. Unique, so a user can administer at most one branch. Cleared when
    /// that admin is deleted, so a new one can be created.
    /// </summary>
    public Guid? AdminUserId { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public User? Admin { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
