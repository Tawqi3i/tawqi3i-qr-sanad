namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;

using Tawqi3i.QrSanad.Common.Enums;

public class SecurityAuditLogsQuery : PagedQuery
{
    public Guid? UserId { get; set; }

    public SecurityEventType? EventType { get; set; }

    /// <summary>
    /// Inclusive, UTC.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Exclusive, UTC.
    /// </summary>
    public DateTime? To { get; set; }
}
