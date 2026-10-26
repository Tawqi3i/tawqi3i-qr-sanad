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

    private FakeAccessProvider accessProvider = null!;
    private AccessGuard guard = null!;

    [SetUp]
    public void SetUp()
    {
        this.accessProvider = new FakeAccessProvider();
        this.guard = new AccessGuard(this.accessProvider);
    }

    [Test]
    public async Task SuperAdmin_ManagesAdminsAndUsersOfAnyBranch()
    {
        var superAdmin = Access(UserType.SuperAdmin, null);

        Assert.That(await this.guard.CanManage(superAdmin, User(UserType.Admin, BranchA), default), Is.True);
        Assert.That(await this.guard.CanManage(superAdmin, User(UserType.User, BranchB), default), Is.True);
    }

    [Test]
    public async Task NobodyManagesTheSuperAdmin()
    {
        var target = User(UserType.SuperAdmin, null);

        Assert.That(await this.guard.CanManage(Access(UserType.SuperAdmin, null), target, default), Is.False);
        Assert.That(await this.guard.CanManage(Access(UserType.Admin, BranchA), target, default), Is.False);
    }

    [Test]
    public async Task Admin_ManagesUsersOfOwnBranchOnly()
    {
        var admin = Access(UserType.Admin, BranchA, PermissionCodes.Users.View);

        Assert.That(await this.guard.CanManage(admin, User(UserType.User, BranchA), default), Is.True);
        Assert.That(await this.guard.CanManage(admin, User(UserType.User, BranchB), default), Is.False);
    }

    [Test]
    public async Task Admin_CannotManageAnotherAdmin()
    {
        var admin = Access(UserType.Admin, BranchA);

        Assert.That(await this.guard.CanManage(admin, User(UserType.Admin, BranchA), default), Is.False);
    }

    [Test]
    public async Task DelegatedUser_ManagesOnlyUsersWithNoMoreAccessThanThemselves()
    {
        var delegated = Access(UserType.User, BranchA, PermissionCodes.Users.View, PermissionCodes.Users.Update);
        var weaker = User(UserType.User, BranchA);
        var stronger = User(UserType.User, BranchA);

        this.accessProvider.Add(Access(UserType.User, BranchA, PermissionCodes.Users.View, userId: weaker.Id));
        this.accessProvider.Add(Access(UserType.User, BranchA, PermissionCodes.Users.Delete, userId: stronger.Id));

        Assert.That(await this.guard.CanManage(delegated, weaker, default), Is.True);
        Assert.That(await this.guard.CanManage(delegated, stronger, default), Is.False);
    }

    [Test]
    public void CannotGrantPermissionsNotHeld()
    {
        var admin = Access(UserType.Admin, BranchA, PermissionCodes.Users.View);

        Assert.That(AccessGuard.CanGrant(admin, [PermissionCodes.Users.View]), Is.True);
        Assert.That(AccessGuard.CanGrant(admin, [PermissionCodes.Users.Delete]), Is.False);
        Assert.That(AccessGuard.CanGrant(Access(UserType.SuperAdmin, null), PermissionCodes.AllCodes), Is.True);
    }

    [Test]
    public void OnlyAdminsMayRevokePermissionsTheyDoNotHold()
    {
        var admin = Access(UserType.Admin, BranchA);
        var delegated = Access(UserType.User, BranchA, PermissionCodes.Users.AssignPermissions);

        Assert.That(AccessGuard.CanChangePermissions(admin, [], [PermissionCodes.Users.Delete]), Is.True);
        Assert.That(AccessGuard.CanChangePermissions(delegated, [], [PermissionCodes.Users.Delete]), Is.False);
    }

    [Test]
    public void VisibleTo_HidesOtherBranchesAndTheSuperAdmin()
    {
        var users = new[]
        {
            User(UserType.SuperAdmin, null),
            User(UserType.Admin, BranchA),
            User(UserType.User, BranchA),
            User(UserType.User, BranchB),
        }.AsQueryable();

        var seenByAdmin = AccessGuard.VisibleTo(users, Access(UserType.Admin, BranchA)).ToList();
        var seenBySuperAdmin = AccessGuard.VisibleTo(users, Access(UserType.SuperAdmin, null)).ToList();

        Assert.That(seenByAdmin, Has.Count.EqualTo(2));
        Assert.That(seenByAdmin.All(a => a.BranchId == BranchA), Is.True);
        Assert.That(seenBySuperAdmin, Has.Count.EqualTo(4));
    }

    [Test]
    public void ScansVisibleTo_FollowsTheAuditLogPermission()
    {
        var scanner = Guid.NewGuid();
        var logs = new[]
        {
            Log(SecurityEventType.QrScanSucceeded, BranchA, scanner),
            Log(SecurityEventType.QrScanFailed, BranchA, Guid.NewGuid()),
            Log(SecurityEventType.QrScanSucceeded, BranchB, Guid.NewGuid()),
            Log(SecurityEventType.LoginSucceeded, BranchA, scanner),
        }.AsQueryable();

        var seenBySuperAdmin = AccessGuard.ScansVisibleTo(logs, Access(UserType.SuperAdmin, null)).ToList();
        var seenByAuditor = AccessGuard.ScansVisibleTo(logs, Access(UserType.Admin, BranchA, PermissionCodes.AuditLogs.View)).ToList();
        var seenByScanner = AccessGuard.ScansVisibleTo(logs, Access(UserType.User, BranchA, PermissionCodes.ESeal.View, scanner)).ToList();

        Assert.That(seenBySuperAdmin, Has.Count.EqualTo(3));
        Assert.That(seenByAuditor, Has.Count.EqualTo(2));
        Assert.That(seenByAuditor.All(a => a.BranchId == BranchA), Is.True);
        Assert.That(seenByScanner, Has.Count.EqualTo(1));
        Assert.That(seenByScanner[0].UserId, Is.EqualTo(scanner));
    }

    [Test]
    public void HasPermission_SuperAdminHoldsAllAndUserOnlyWhatWasGranted()
    {
        var superAdmin = Access(UserType.SuperAdmin, null);

        Assert.That(superAdmin.HasPermission("anything.at_all"), Is.True);
        Assert.That(Access(UserType.User, BranchA).HasPermission(PermissionCodes.Users.View), Is.False);
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

    private static UserAccess Access(UserType type, Guid? branchId, string permission, Guid userId)
    {
        return Access(type, branchId, [permission], userId);
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

    private sealed class FakeAccessProvider : IUserAccessProvider
    {
        private readonly Dictionary<Guid, UserAccess> items = new();

        public void Add(UserAccess access) => this.items[access.UserId] = access;

        public Task<UserAccess?> Get(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.items.GetValueOrDefault(userId));
        }

        public Task Invalidate(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Invalidate(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
