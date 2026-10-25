namespace Tawqi3i.QrSanad.Domain.Services.V1.Audit;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

public interface ISecurityAuditLogsService
{
    /// <summary>
    /// The SuperAdmin sees every entry; everyone else only their own branch's.
    /// </summary>
    Task<PagedResponse<SecurityAuditLogViewModel>> GetAll(SecurityAuditLogsQuery query, RequestUser actor, CancellationToken cancellationToken = default);
}
