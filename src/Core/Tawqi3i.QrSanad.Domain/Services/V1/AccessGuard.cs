namespace Tawqi3i.QrSanad.Domain.Services.V1;

using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// The scope rules. [HasPermission] and [SuperAdminOnly] decide whether someone may act at all;
/// this decides on which records:
///  - the SuperAdmin sees everything;
///  - everyone else sees only their own branch;
///  - a device sees only its own scans.
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
    /// The SuperAdmin and the admins: what /users lists. Reserved to the SuperAdmin.
    /// </summary>
    public static IQueryable<User> Users(IQueryable<User> users)
    {
        return users.Where(a => a.UserType != UserType.Device);
    }

    /// <summary>
    /// Restricts a query to the devices the actor may see. Devices outside it are reported as not
    /// found, so other branches' devices cannot even be confirmed to exist.
    /// </summary>
    public static IQueryable<User> DevicesVisibleTo(IQueryable<User> users, UserAccess actor)
    {
        var devices = users.Where(a => a.UserType == UserType.Device);

        if (actor.IsSuperAdmin)
        {
            return devices;
        }

        var branchId = actor.BranchId;
        return devices.Where(a => a.BranchId == branchId && branchId != null);
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

    /// <summary>
    /// Restricts a query to the QR scans the actor may see: the audit entries they may see when they
    /// hold audit_logs.view, otherwise only their own scans (none, unless they are a device).
    /// </summary>
    public static IQueryable<SecurityAuditLog> ScansVisibleTo(IQueryable<SecurityAuditLog> logs, UserAccess actor)
    {
        var scans = logs.Where(a => a.EventType == SecurityEventType.QrScanSucceeded || a.EventType == SecurityEventType.QrScanFailed);
        if (actor.HasPermission(PermissionCodes.AuditLogs.View))
        {
            return VisibleTo(scans, actor);
        }

        var userId = actor.UserId;
        return scans.Where(a => a.UserId == userId);
    }
}
