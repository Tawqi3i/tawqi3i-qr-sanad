namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// Every query excludes soft-deleted users.
/// </summary>
public interface IUsersRepository : IRepository<User>, IUnitOnlyRepository<User>
{
    /// <summary>
    /// No-tracking, with the branch loaded: login state is changed with set-based updates, not
    /// by saving this instance.
    /// </summary>
    Task<User?> FindByUserNameOrEmail(string normalizedValue, CancellationToken cancellationToken = default);

    Task<bool> UserNameExists(string normalizedUserName, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    Task<bool> EmailExists(string normalizedEmail, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// What the authorization layer needs to know about a user, or null if they no longer exist.
    /// </summary>
    Task<UserAccess?> GetAccess(Guid userId, CancellationToken cancellationToken = default);

    Task<ICollection<Guid>> GetIdsInBranch(Guid branchId, CancellationToken cancellationToken = default);

    Task<ICollection<Guid>> GetAllIds(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set-based: flags every user to choose a new password at their next request. Returns how
    /// many users were not already flagged.
    /// </summary>
    Task<int> RequirePasswordChangeForAll(DateTime now, Guid actorId, CancellationToken cancellationToken = default);
}
