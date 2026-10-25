namespace Tawqi3i.QrSanad.Identity;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Persistence;

/// <summary>
/// Two cache levels: a per-request dictionary (authentication and each [HasPermission] check
/// in the same request share one lookup) over IDistributedCache (memory, or Redis when
/// configured, which makes invalidation reach every instance).
/// </summary>
internal sealed class UserAccessProvider : IUserAccessProvider
{
    private const string KeyPrefix = "access:";

    private readonly IUsersRepository usersRepository;
    private readonly IDistributedCache cache;
    private readonly AuthSettings settings;
    private readonly Dictionary<Guid, UserAccess?> requestCache = new();

    public UserAccessProvider(IUsersRepository usersRepository, IDistributedCache cache, AuthSettings settings)
    {
        this.usersRepository = usersRepository;
        this.cache = cache;
        this.settings = settings;
    }

    public async Task<UserAccess?> Get(Guid userId, CancellationToken cancellationToken = default)
    {
        if (this.requestCache.TryGetValue(userId, out var memoised))
        {
            return memoised;
        }

        var key = Key(userId);
        var cached = await this.cache.GetAsync(key, cancellationToken);

        UserAccess? access;

        if (cached != null)
        {
            access = JsonSerializer.Deserialize<UserAccess>(cached);
        }
        else
        {
            access = await this.usersRepository.GetAccess(userId, cancellationToken);

            if (access != null)
            {
                await this.cache.SetAsync(
                    key,
                    JsonSerializer.SerializeToUtf8Bytes(access),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = this.settings.AccessCacheDuration },
                    cancellationToken);
            }
        }

        this.requestCache[userId] = access;
        return access;
    }

    public async Task Invalidate(Guid userId, CancellationToken cancellationToken = default)
    {
        this.requestCache.Remove(userId);
        await this.cache.RemoveAsync(Key(userId), cancellationToken);
    }

    public async Task Invalidate(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            await this.Invalidate(userId, cancellationToken);
        }
    }

    private static string Key(Guid userId) => KeyPrefix + userId.ToString("N");
}
