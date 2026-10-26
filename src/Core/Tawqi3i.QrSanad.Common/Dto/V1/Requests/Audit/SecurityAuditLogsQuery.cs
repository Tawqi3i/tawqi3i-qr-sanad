namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;

using Tawqi3i.QrSanad.Common.Enums;

public class SecurityAuditLogsQuery : AuditLogsFilter
{
    public SecurityEventType? EventType { get; set; }
}
