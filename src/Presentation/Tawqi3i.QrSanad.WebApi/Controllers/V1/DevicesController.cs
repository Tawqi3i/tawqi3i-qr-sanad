namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Devices;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// The scanning stations. The SuperAdmin manages the devices of every branch; an admin those of
/// their own branch. Devices outside the caller's branch answer 404.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
public class DevicesController : ControllerBase
{
    private readonly IDevicesService devicesService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public DevicesController(IDevicesService devicesService, IStringLocalizer<SharedResource> localizer)
    {
        this.devicesService = devicesService;
        this.localizer = localizer;
    }

    [HttpGet, HasPermission(PermissionCodes.Devices.View)]
    [ProducesResponseType(typeof(PagedResponse<UserViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UsersQuery query, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.GetAll(query, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpGet("{id:guid}"), HasPermission(PermissionCodes.Devices.View)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.Get(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// SuperAdmin: adds a device to the given branchId. Admin: adds it to their own branch.
    /// </summary>
    [HttpPost, HasPermission(PermissionCodes.Devices.Create)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] DeviceCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.Create(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpPut("{id:guid}"), HasPermission(PermissionCodes.Devices.Update)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DeviceUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.Update(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Activates or disables a device. Disabling ends its session.
    /// </summary>
    [HttpPut("{id:guid}/status"), HasPermission(PermissionCodes.Devices.Update)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] UserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.SetStatus(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Clears a lockout caused by failed login attempts.
    /// </summary>
    [HttpPut("{id:guid}/unlock"), HasPermission(PermissionCodes.Devices.Update)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.Unlock(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Sets a new password for the device and ends its session.
    /// </summary>
    [HttpPut("{id:guid}/password"), HasPermission(PermissionCodes.Devices.ResetPassword)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.ResetPassword(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Soft-deletes the device and ends its session.
    /// </summary>
    [HttpDelete("{id:guid}"), HasPermission(PermissionCodes.Devices.Delete)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.devicesService.Delete(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
