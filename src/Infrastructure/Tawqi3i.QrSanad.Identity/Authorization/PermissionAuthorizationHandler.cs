namespace Tawqi3i.QrSanad.Identity.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// Evaluates [HasPermission] and [SuperAdminOnly] against the live, cached access snapshot.
/// While a password change is pending, both refuse with a reason the client can show.
/// </summary>
internal sealed class PermissionAuthorizationHandler : IAuthorizationHandler
{
    private readonly IUserAccessProvider accessProvider;

    public PermissionAuthorizationHandler(IUserAccessProvider accessProvider)
    {
        this.accessProvider = accessProvider;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var requirements = context.PendingRequirements
            .Where(a => a is PermissionRequirement or SuperAdminRequirement)
            .ToList();

        if (requirements.Count == 0)
        {
            return;
        }

        if (!Guid.TryParse(context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId))
        {
            return;
        }

        var access = await this.accessProvider.Get(userId);

        if (access is not { IsActive: true })
        {
            return;
        }

        // The claim covers tokens issued while the flag was set; the live flag covers tokens
        // issued before it, e.g. when the password policy changes.
        if (access.MustChangePassword || context.User.HasClaim(a => a.Type == CustomClaimNames.PasswordChangeRequired))
        {
            context.Fail(new AuthorizationFailureReason(this, ResponseMessages.PasswordChangeRequired));
            return;
        }

        foreach (var requirement in requirements)
        {
            var satisfied = requirement switch
            {
                PermissionRequirement permission => access.HasPermission(permission.Permission),
                SuperAdminRequirement => access.IsSuperAdmin,
                _ => false
            };

            if (satisfied)
            {
                context.Succeed(requirement);
            }
        }
    }
}
