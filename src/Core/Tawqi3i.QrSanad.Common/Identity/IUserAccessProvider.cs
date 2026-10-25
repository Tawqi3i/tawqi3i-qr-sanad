namespace Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// Cached lookup of <see cref="UserAccess"/>. Anything that changes a user's status, security
/// stamp, roles or permissions must call <see cref="Invalidate(Guid, CancellationToken)"/>
/// afterwards, or the change only takes effect when the cache entry expires.
/// </summary>
public interface IUserAccessProvider
{
    Task<UserAccess?> Get(Guid userId, CancellationToken cancellationToken = default);

    Task Invalidate(Guid userId, CancellationToken cancellationToken = default);

    Task Invalidate(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
