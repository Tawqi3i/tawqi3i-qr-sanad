namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;

/// <summary>
/// The filters shared by every audit log listing.
/// </summary>
public class AuditLogsFilter : PagedQuery
{
    public Guid? UserId { get; set; }

    /// <summary>
    /// Inclusive, UTC.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Exclusive, UTC.
    /// </summary>
    public DateTime? To { get; set; }
}
