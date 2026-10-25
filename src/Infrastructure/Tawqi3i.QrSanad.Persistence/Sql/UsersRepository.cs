namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class UsersRepository : RepositoryBase<User>, IUsersRepository
{
    public UsersRepository(QrSanadSqlContext context)
        : base(context)
    {
    }

    public override IQueryable<User> GetQueryable() => base.GetQueryable().Where(a => !a.IsDeleted);

    public override IQueryable<User> GetTrackedQueryable() => base.GetTrackedQueryable().Where(a => !a.IsDeleted);

    // Usernames cannot contain '@', so the identifier is an email exactly when it has one.
    public Task<User?> FindByUserNameOrEmail(string normalizedValue, CancellationToken cancellationToken = default)
    {
        var users = this.GetQueryable().Include(a => a.Branch);

        return normalizedValue.Contains('@')
            ? users.FirstOrDefaultAsync(a => a.NormalizedEmail == normalizedValue, cancellationToken)
            : users.FirstOrDefaultAsync(a => a.NormalizedUserName == normalizedValue, cancellationToken);
    }

    // Deleted users are included on purpose: their usernames and emails stay reserved.
    public Task<bool> UserNameExists(string normalizedUserName, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        return this.context.Users.AnyAsync(a => a.NormalizedUserName == normalizedUserName && a.Id != excludeUserId, cancellationToken);
    }

    public Task<bool> EmailExists(string normalizedEmail, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        return this.context.Users.AnyAsync(a => a.NormalizedEmail == normalizedEmail && a.Id != excludeUserId, cancellationToken);
    }

    public async Task<UserAccess?> GetAccess(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await this.GetQueryable()
            .Where(a => a.Id == userId)
            .Select(a => new
            {
                a.UserType,
                a.BranchId,
                a.SecurityStamp,
                a.Status,
                a.MustChangePassword,
                BranchActive = a.Branch == null || (a.Branch.Status == BranchStatus.Active && !a.Branch.IsDeleted),
                Permissions = a.UserPermissions.Select(p => p.Permission.Code).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return null;
        }

        var permissions = user.UserType == UserType.SuperAdmin ? PermissionCodes.AllCodes : user.Permissions;

        return new UserAccess
        {
            UserId = userId,
            UserType = user.UserType,
            BranchId = user.BranchId,
            SecurityStamp = user.SecurityStamp,
            IsActive = user.Status == UserStatus.Active && user.BranchActive,
            MustChangePassword = user.MustChangePassword,
            Permissions = permissions.Order(StringComparer.Ordinal).ToArray()
        };
    }

    public async Task<ICollection<Guid>> GetIdsInBranch(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await this.GetQueryable().Where(a => a.BranchId == branchId).Select(a => a.Id).ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Guid>> GetAllIds(CancellationToken cancellationToken = default)
    {
        return await this.GetQueryable().Select(a => a.Id).ToListAsync(cancellationToken);
    }

    public Task<int> RequirePasswordChangeForAll(DateTime now, Guid actorId, CancellationToken cancellationToken = default)
    {
        var stamp = Generator.NewStamp();

        return this.Update(
            a => !a.MustChangePassword,
            setters => setters
                .SetProperty(a => a.MustChangePassword, true)
                .SetProperty(a => a.ConcurrencyStamp, stamp)
                .SetProperty(a => a.UpdatedAt, now)
                .SetProperty(a => a.UpdatedBy, actorId),
            cancellationToken);
    }
}
