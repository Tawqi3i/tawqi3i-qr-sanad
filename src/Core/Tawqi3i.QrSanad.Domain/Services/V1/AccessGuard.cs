namespace Tawqi3i.QrSanad.Domain.Services.V1;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// The hierarchy rules. [HasPermission] decides whether someone may manage users at all; this
/// decides whom and how far:
///  - the SuperAdmin sees and manages everyone except itself;
///  - everyone else sees only their own branch and manages only its plain users;
///  - a branch admin manages every user of the branch; a user who was delegated user
///    management manages only users whose permissions they also hold;
///  - nobody grants a permission they do not hold, and only admins may revoke one they do not hold.
/// </summary>
internal sealed class AccessGuard
{
    private readonly IUserAccessProvider accessProvider;

    public AccessGuard(IUserAccessProvider accessProvider)
    {
        this.accessProvider = accessProvider;
    }

    public async Task<UserAccess> GetActorAccess(RequestUser actor, CancellationToken cancellationToken)
    {
        return await this.accessProvider.Get(actor.Id, cancellationToken)
            ?? throw new AppException(Response.Unauthorized());
    }

    /// <summary>
    /// Restricts a query to the users the actor may see. Users outside it are reported as not
    /// found, so other branches' users cannot even be confirmed to exist.
    /// </summary>
    public static IQueryable<User> VisibleTo(IQueryable<User> users, UserAccess actor)
    {
        if (actor.IsSuperAdmin)
        {
            return users;
        }

        var branchId = actor.BranchId;
        return users.Where(a => a.BranchId == branchId && branchId != null && a.UserType != UserType.SuperAdmin);
    }

    /// <summary>
    /// Restricts a query to the audit entries the actor may see: every entry for the SuperAdmin,
    /// otherwise only their own branch's.
    /// </summary>
    public static IQueryable<SecurityAuditLog> VisibleTo(IQueryable<SecurityAuditLog> logs, UserAccess actor)
    {
        if (actor.IsSuperAdmin)
        {
            return logs;
        }

        var branchId = actor.BranchId;
        return logs.Where(a => a.BranchId == branchId && branchId != null);
    }

    public static bool CanGrant(UserAccess actor, IEnumerable<string> permissionCodes)
    {
        return actor.IsSuperAdmin || permissionCodes.All(actor.HasPermission);
    }

    public static bool CanChangePermissions(UserAccess actor, IEnumerable<string> added, IEnumerable<string> removed)
    {
        return actor.IsSuperAdmin
            || (added.All(actor.HasPermission) && (actor.UserType == UserType.Admin || removed.All(actor.HasPermission)));
    }

    public async Task<bool> CanManage(UserAccess actor, User target, CancellationToken cancellationToken)
    {
        if (target.UserType == UserType.SuperAdmin)
        {
            return false;
        }

        if (actor.IsSuperAdmin)
        {
            return true;
        }

        if (target.BranchId != actor.BranchId || target.UserType != UserType.User)
        {
            return false;
        }

        if (actor.UserType == UserType.Admin)
        {
            return true;
        }

        var targetAccess = await this.accessProvider.Get(target.Id, cancellationToken);
        return targetAccess == null || CanGrant(actor, targetAccess.Permissions);
    }
}
