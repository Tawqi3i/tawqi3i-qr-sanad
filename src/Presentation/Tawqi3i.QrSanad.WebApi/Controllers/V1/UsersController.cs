namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Users;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// The SuperAdmin manages the branch admins here. Devices are managed through /devices and
/// answer 404 here.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[SuperAdminOnly]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
public class UsersController : ControllerBase
{
    private readonly IUsersService usersService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public UsersController(IUsersService usersService, IStringLocalizer<SharedResource> localizer)
    {
        this.usersService = usersService;
        this.localizer = localizer;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UsersQuery query, CancellationToken cancellationToken)
    {
        var result = await this.usersService.GetAll(query, cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Get(id, cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Creates the admin of a branch (one per branch).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Create(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Update(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Activates or disables an admin. Disabling ends all of their sessions.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] UserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.SetStatus(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Clears a lockout caused by failed login attempts.
    /// </summary>
    [HttpPut("{id:guid}/unlock")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Unlock(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Sets a new password for an admin and ends all of their sessions.
    /// </summary>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.ResetPassword(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Replaces the admin's permissions with exactly the given set.
    /// </summary>
    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] PermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.SetPermissions(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Soft-deletes the admin, ends all of their sessions and frees the branch for a new admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Delete(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
