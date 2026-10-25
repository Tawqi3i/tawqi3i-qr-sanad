namespace Tawqi3i.QrSanad.Identity;

public class AuthSettings
{
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Consecutive wrong passwords before the account is locked.
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How long a user's roles, permissions and status are cached. Changes made through the API
    /// invalidate the cache straight away; this bounds staleness for anything else.
    /// </summary>
    public TimeSpan AccessCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
}
