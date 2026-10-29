namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Domain.Services.V1;

public class AccessGuardTests
{
    private static readonly Guid BranchA = Guid.NewGuid();
    private static readonly Guid BranchB = Guid.NewGuid();

    private static readonly IQueryable<User> Accounts = new[]
    {
        User(UserType.SuperAdmin, null),
        User(UserType.Admin, BranchA),
        User(UserType.Device, BranchA),
        User(UserType.Device, BranchA),
        User(UserType.Device, BranchB),
    }.AsQueryable();

    [Test]
    public void Users_ListsEveryoneButDevices()
    {
        var users = AccessGuard.Users(Accounts).ToList();

        Assert.That(users, Has.Count.EqualTo(2));
        Assert.That(users.Any(a => a.UserType == UserType.Device), Is.False);
    }

    [Test]
    public void DevicesVisibleTo_SuperAdminSeesEveryBranch()
    {
        var devices = AccessGuard.DevicesVisibleTo(Accounts, Access(UserType.SuperAdmin, null)).ToList();

        Assert.That(devices, Has.Count.EqualTo(3));
        Assert.That(devices.All(a => a.UserType == UserType.Device), Is.True);
    }

    [Test]
    public void DevicesVisibleTo_AdminSeesOwnBranchOnly()
    {
        var devices = AccessGuard.DevicesVisibleTo(Accounts, Access(UserType.Admin, BranchA, PermissionCodes.Devices.View)).ToList();

        Assert.That(devices, Has.Count.EqualTo(2));
        Assert.That(devices.All(a => a.BranchId == BranchA && a.UserType == UserType.Device), Is.True);
    }

    [Test]
    public void ScansVisibleTo_FollowsTheAuditLogPermission()
    {
        var device = Guid.NewGuid();
        var logs = new[]
        {
            Log(SecurityEventType.QrScanSucceeded, BranchA, device),
            Log(SecurityEventType.QrScanFailed, BranchA, Guid.NewGuid()),
            Log(SecurityEventType.QrScanSucceeded, BranchB, Guid.NewGuid()),
            Log(SecurityEventType.LoginSucceeded, BranchA, device),
        }.AsQueryable();

        var seenBySuperAdmin = AccessGuard.ScansVisibleTo(logs, Access(UserType.SuperAdmin, null)).ToList();
        var seenByAuditor = AccessGuard.ScansVisibleTo(logs, Access(UserType.Admin, BranchA, PermissionCodes.AuditLogs.View)).ToList();
        var seenByAdmin = AccessGuard.ScansVisibleTo(logs, Access(UserType.Admin, BranchA)).ToList();
        var seenByDevice = AccessGuard.ScansVisibleTo(logs, Access(UserType.Device, BranchA, [], device)).ToList();

        Assert.That(seenBySuperAdmin, Has.Count.EqualTo(3));
        Assert.That(seenByAuditor, Has.Count.EqualTo(2));
        Assert.That(seenByAuditor.All(a => a.BranchId == BranchA), Is.True);
        Assert.That(seenByAdmin, Is.Empty);
        Assert.That(seenByDevice, Has.Count.EqualTo(1));
        Assert.That(seenByDevice[0].UserId, Is.EqualTo(device));
    }

    [Test]
    public void HasPermission_SuperAdminHoldsAllAndDeviceNone()
    {
        Assert.That(Access(UserType.SuperAdmin, null).HasPermission("anything.at_all"), Is.True);
        Assert.That(Access(UserType.Device, BranchA).HasPermission(PermissionCodes.Devices.View), Is.False);
        Assert.That(Access(UserType.Device, BranchA).IsDevice, Is.True);
    }

    private static User User(UserType type, Guid? branchId)
    {
        return new User { Id = Guid.NewGuid(), UserType = type, BranchId = branchId };
    }

    private static SecurityAuditLog Log(SecurityEventType eventType, Guid branchId, Guid userId)
    {
        return new SecurityAuditLog { Id = Guid.NewGuid(), EventType = eventType, BranchId = branchId, UserId = userId };
    }

    private static UserAccess Access(UserType type, Guid? branchId, params string[] permissions)
    {
        return Access(type, branchId, permissions, Guid.NewGuid());
    }

    private static UserAccess Access(UserType type, Guid? branchId, string[] permissions, Guid userId)
    {
        return new UserAccess
        {
            UserId = userId,
            UserType = type,
            BranchId = branchId,
            IsActive = true,
            Permissions = permissions
        };
    }
}
