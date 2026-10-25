namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Users;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// The SuperAdmin manages admins and users of every branch; a branch admin manages the users
/// of their own branch. Users outside the caller's branch answer 404.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
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

    [HttpGet, HasPermission(PermissionCodes.Users.View)]
    [ProducesResponseType(typeof(PagedResponse<UserViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UsersQuery query, CancellationToken cancellationToken)
    {
        var result = await this.usersService.GetAll(query, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpGet("{id:guid}"), HasPermission(PermissionCodes.Users.View)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Get(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// SuperAdmin: creates an admin (one per branch) or a user, for the given branchId.
    /// Admin: creates a user in their own branch.
    /// </summary>
    [HttpPost, HasPermission(PermissionCodes.Users.Create)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Create(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpPut("{id:guid}"), HasPermission(PermissionCodes.Users.Update)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Update(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Activates or disables a user. Disabling ends all of their sessions.
    /// </summary>
    [HttpPut("{id:guid}/status"), HasPermission(PermissionCodes.Users.Update)]
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
    [HttpPut("{id:guid}/unlock"), HasPermission(PermissionCodes.Users.Update)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Unlock(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Sets a new password for another user and ends all of their sessions.
    /// </summary>
    [HttpPut("{id:guid}/password"), HasPermission(PermissionCodes.Users.ResetPassword)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.ResetPassword(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Replaces the user's permissions with exactly the given set. Only permissions the caller
    /// holds can be granted.
    /// </summary>
    [HttpPut("{id:guid}/permissions"), HasPermission(PermissionCodes.Users.AssignPermissions)]
    [ProducesResponseType(typeof(Response<UserDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] PermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await this.usersService.SetPermissions(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Soft-deletes the user and ends all of their sessions. Deleting a branch admin frees the
    /// branch for a new admin.
    /// </summary>
    [HttpDelete("{id:guid}"), HasPermission(PermissionCodes.Users.Delete)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.usersService.Delete(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
