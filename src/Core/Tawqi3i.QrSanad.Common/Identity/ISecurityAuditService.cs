namespace Tawqi3i.QrSanad.Common.Identity;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Enums;

public interface ISecurityAuditService
{
    /// <summary>
    /// Stages an audit entry on the unit of work. It is persisted by the caller's next
    /// SaveChanges, in the same transaction as the change it describes.
    /// </summary>
    void Record(
        SecurityEventType eventType,
        ClientInfo client,
        Guid? userId = null,
        string? userName = null,
        Guid? actorId = null,
        Guid? branchId = null,
        string? details = null);
}
