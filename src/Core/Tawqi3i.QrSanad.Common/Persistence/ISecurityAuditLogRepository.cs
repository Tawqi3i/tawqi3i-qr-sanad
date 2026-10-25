namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

public interface ISecurityAuditLogRepository : IRepository<SecurityAuditLog>, IUnitOnlyRepository<SecurityAuditLog>
{
}
