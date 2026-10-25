namespace Tawqi3i.QrSanad.WebApi.Settings;

public class SecuritySettings
{
    /// <summary>
    /// Redirect HTTP to HTTPS and send HSTS. Turn off only when TLS ends at a proxy that
    /// already does both.
    /// </summary>
    public bool EnforceHttps { get; set; } = true;

    /// <summary>
    /// Browser origins allowed by CORS. Empty means no cross-origin browser access.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Reverse proxies whose X-Forwarded-For / X-Forwarded-Proto headers are trusted. Without
    /// this, every request appears to come from the proxy, and rate limits and audit IPs are wrong.
    /// </summary>
    public string[] KnownProxies { get; set; } = [];

    public bool EnableSwagger { get; set; }
}

public class RateLimitingSettings
{
    /// <summary>
    /// Login and refresh, per client IP.
    /// </summary>
    public FixedWindowSettings Auth { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    /// <summary>
    /// Every request, per client IP.
    /// </summary>
    public FixedWindowSettings Global { get; set; } = new() { PermitLimit = 300, WindowSeconds = 60 };
}

public class FixedWindowSettings
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }
}
