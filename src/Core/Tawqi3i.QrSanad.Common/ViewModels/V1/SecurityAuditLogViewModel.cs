namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

using Tawqi3i.QrSanad.Common.Enums;

public class SecurityAuditLogViewModel
{
    public Guid Id { get; set; }

    public SecurityEventType EventType { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public Guid? ActorId { get; set; }

    public Guid? BranchId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }
}
