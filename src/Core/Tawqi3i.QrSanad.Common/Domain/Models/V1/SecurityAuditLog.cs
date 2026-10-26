namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

using Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// Append-only record of authentication and access-control events. UserId is not a foreign key,
/// so entries outlive the user they describe.
/// </summary>
public class SecurityAuditLog
{
    public Guid Id { get; set; }

    public SecurityEventType EventType { get; set; }

    /// <summary>
    /// The user the event is about (the target), when known.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The identifier that was presented, e.g. the username typed into a failed login.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The user who performed the action, when different from the target.
    /// </summary>
    public Guid? ActorId { get; set; }

    /// <summary>
    /// The branch the event belongs to, so a branch admin sees only their branch's entries.
    /// </summary>
    public Guid? BranchId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Details { get; set; }

    /// <summary>
    /// The identity card a successful QR scan returned. Null for other events and documents.
    /// </summary>
    public IdentityCard? IdentityCard { get; set; }

    public DateTime CreatedAt { get; set; }
}
