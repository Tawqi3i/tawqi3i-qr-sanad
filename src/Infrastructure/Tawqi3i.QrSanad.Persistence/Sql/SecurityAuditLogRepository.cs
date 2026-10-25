namespace Tawqi3i.QrSanad.Persistence.Sql;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class SecurityAuditLogRepository : RepositoryBase<SecurityAuditLog>, ISecurityAuditLogRepository
{
    public SecurityAuditLogRepository(QrSanadSqlContext context)
        : base(context)
    {
    }
}
