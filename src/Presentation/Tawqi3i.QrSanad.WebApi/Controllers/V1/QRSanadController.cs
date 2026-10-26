namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Audit;
using Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// SANAD QR documents, fetched directly from the MoDEE e-seal service, and their scan
/// statistics. The permission keeps its original code, eseal.view: its id is derived from the
/// code, so renaming it would drop every existing grant.
/// </summary>
[Route("api/v1/qr-sanad")]
[ApiController]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
public class QRSanadController : ControllerBase
{
    private readonly IQRSanadService qrSanadService;
    private readonly IScanStatsService scanStatsService;
    private readonly ISecurityAuditLogsService auditLogsService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public QRSanadController(
        IQRSanadService qrSanadService,
        IScanStatsService scanStatsService,
        ISecurityAuditLogsService auditLogsService,
        IStringLocalizer<SharedResource> localizer)
    {
        this.qrSanadService = qrSanadService;
        this.scanStatsService = scanStatsService;
        this.auditLogsService = auditLogsService;
        this.localizer = localizer;
    }

    /// <summary>
    /// The MoDEE e-sealed document (base64 PAdES-T PDF) for a SANAD digital document QR code.
    /// Every scan is written to the audit log as qrScanSucceeded or qrScanFailed.
    /// </summary>
    [HttpPost("sealed-doc"), HasPermission(PermissionCodes.ESeal.View)]
    [ProducesResponseType(typeof(Response<SealedDocViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetSealedDocument([FromBody] SealedDocRequest request, CancellationToken cancellationToken)
    {
        var result = await this.qrSanadService.GetSealedDocument(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// The QR scans, newest first, with the identity card holder when there was one: all of them
    /// for the SuperAdmin, the branch's for whoever holds audit_logs.view, otherwise the caller's
    /// own. Search by national number or name.
    /// </summary>
    [HttpGet("scans"), HasPermission(PermissionCodes.ESeal.View)]
    [ProducesResponseType(typeof(PagedResponse<SecurityAuditLogViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetScans([FromQuery] ScansQuery query, CancellationToken cancellationToken)
    {
        var result = await this.auditLogsService.GetScans(query, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// QR scan statistics for the dashboard: totals, per day, top users and, for the SuperAdmin,
    /// per branch. Defaults to the last 30 days; at most 92. Admins see their own branch only.
    /// </summary>
    [HttpGet("stats"), HasPermission(PermissionCodes.AuditLogs.View)]
    [ProducesResponseType(typeof(Response<ScanStatsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats([FromQuery] ScanStatsQuery query, CancellationToken cancellationToken)
    {
        var result = await this.scanStatsService.Get(query, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
