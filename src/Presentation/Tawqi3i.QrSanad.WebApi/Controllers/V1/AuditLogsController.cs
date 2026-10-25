namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Audit;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

[Route("api/v1/audit-logs")]
[ApiController]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
public class AuditLogsController : ControllerBase
{
    private readonly ISecurityAuditLogsService auditLogsService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public AuditLogsController(ISecurityAuditLogsService auditLogsService, IStringLocalizer<SharedResource> localizer)
    {
        this.auditLogsService = auditLogsService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Logins, lockouts, password changes and every change to branches, users and permissions,
    /// newest first. Admins see their own branch only.
    /// </summary>
    [HttpGet, HasPermission(PermissionCodes.AuditLogs.View)]
    [ProducesResponseType(typeof(PagedResponse<SecurityAuditLogViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SecurityAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var result = await this.auditLogsService.GetAll(query, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
