namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.PasswordPolicies;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly IPasswordPolicyService passwordPolicyService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public AuthController(IAuthService authService, IPasswordPolicyService passwordPolicyService, IStringLocalizer<SharedResource> localizer)
    {
        this.authService = authService;
        this.passwordPolicyService = passwordPolicyService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Exchanges a username (or email) and password for an access token and a refresh token.
    /// </summary>
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting(Startup.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(Response<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status423Locked)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await this.authService.Login(request, this.HttpContext.ExtractClient(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Exchanges a refresh token for a new pair. The old refresh token stops working; presenting
    /// it again ends every session of the user.
    /// </summary>
    [HttpPost("refresh"), AllowAnonymous, EnableRateLimiting(Startup.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(Response<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await this.authService.Refresh(request, this.HttpContext.ExtractClient(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Revokes the given refresh token. The access token stays valid until it expires; discard it.
    /// </summary>
    [HttpPost("logout"), Authorize]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await this.authService.Logout(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Ends every session of the caller on every device, including this one.
    /// </summary>
    [HttpPost("logout-all"), Authorize]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var result = await this.authService.LogoutAll(this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// The signed-in user with their roles and effective permissions.
    /// </summary>
    [HttpGet("me"), Authorize]
    [ProducesResponseType(typeof(Response<ProfileViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await this.authService.GetProfile(this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Changes the caller's password, ends their other sessions and returns new tokens.
    /// </summary>
    [HttpPut("password"), Authorize]
    [ProducesResponseType(typeof(Response<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await this.authService.ChangePassword(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// The rules a new password must satisfy, for client-side hints.
    /// </summary>
    [HttpGet("password-policy"), AllowAnonymous]
    [ProducesResponseType(typeof(Response<PasswordPolicyViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PasswordPolicy(CancellationToken cancellationToken)
    {
        var result = await this.passwordPolicyService.Get(cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Replaces the password policy and requires every user, the SuperAdmin included, to change
    /// their password. Until they do, only change-password, profile and logout work.
    /// </summary>
    [HttpPut("password-policy"), SuperAdminOnly]
    [ProducesResponseType(typeof(Response<PasswordPolicyViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePasswordPolicy([FromBody] PasswordPolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await this.passwordPolicyService.Update(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
