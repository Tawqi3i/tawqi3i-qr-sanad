namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Permissions;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

[Route("api/v1/[controller]")]
[ApiController]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionsService permissionsService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public PermissionsController(IPermissionsService permissionsService, IStringLocalizer<SharedResource> localizer)
    {
        this.permissionsService = permissionsService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Every permission code that can be assigned to an admin.
    /// </summary>
    [HttpGet, SuperAdminOnly]
    [ProducesResponseType(typeof(Response<ICollection<PermissionViewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await this.permissionsService.GetAll(cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
