namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Licensing;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// The installed license. An expired license blocks QR scans and new users and branches.
/// </summary>
[Route("api/v1/license")]
[ApiController]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
public class LicenseController : ControllerBase
{
    private readonly ILicenseService licenseService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public LicenseController(ILicenseService licenseService, IStringLocalizer<SharedResource> localizer)
    {
        this.licenseService = licenseService;
        this.localizer = localizer;
    }

    /// <summary>
    /// The license details, its limits and current usage.
    /// </summary>
    [HttpGet, SuperAdminOnly]
    [ProducesResponseType(typeof(Response<LicenseViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await this.licenseService.Get(cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Installs a new license key, e.g. a renewal. It must be valid and not expired.
    /// </summary>
    [HttpPut, SuperAdminOnly]
    [ProducesResponseType(typeof(Response<LicenseViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] LicenseRequest request, CancellationToken cancellationToken)
    {
        var result = await this.licenseService.Update(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
