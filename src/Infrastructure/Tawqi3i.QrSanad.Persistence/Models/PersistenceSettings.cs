namespace Tawqi3i.QrSanad.Persistence.Models;

public class PersistenceSettings
{
    public SqlSettings Sql { get; set; } = new();

    public RedisCacheSettings RedisCache { get; set; } = new();

    public AdminSeedSettings Seed { get; set; } = new();
}

public class RedisCacheSettings
{
    /// <summary>
    /// Leave empty to use an in-process cache. Set it when running more than one instance, so a
    /// disabled user or revoked permission takes effect on every instance at once.
    /// </summary>
    public string? DefaultConnection { get; set; }

    public string InstanceName { get; set; } = "QrSanad:";
}

/// <summary>
/// The first SuperAdmin, created at startup when no system user exists. Supply the password
/// through user-secrets or an environment variable (Persistence__Seed__Password), never
/// through a committed file. Without a password the seeding is skipped.
/// </summary>
public class AdminSeedSettings
{
    public string UserName { get; set; } = "admin";

    public string Email { get; set; } = "admin@qrsanad.local";

    public string DisplayName { get; set; } = "System Administrator";

    public string? Password { get; set; }
}
