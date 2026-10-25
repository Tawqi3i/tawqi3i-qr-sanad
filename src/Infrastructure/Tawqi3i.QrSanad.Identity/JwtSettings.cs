namespace Tawqi3i.QrSanad.Identity;

using System.Text;

public class JwtSettings
{
    public const int MinimumKeyBytes = 32;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// HMAC-SHA256 signing key, at least 32 bytes. Supply it through user-secrets or the
    /// Identity__Jwt__SecretKey environment variable; it must never be committed.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Kept short: a stolen access token is only useful until it expires, while refresh tokens
    /// can be revoked.
    /// </summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.Issuer) || string.IsNullOrWhiteSpace(this.Audience))
        {
            throw new InvalidOperationException("Identity:Jwt:Issuer and Identity:Jwt:Audience must be configured.");
        }

        if (Encoding.UTF8.GetByteCount(this.SecretKey ?? string.Empty) < MinimumKeyBytes)
        {
            throw new InvalidOperationException(
                $"Identity:Jwt:SecretKey must be at least {MinimumKeyBytes} bytes. " +
                "Set it with 'dotnet user-secrets set Identity:Jwt:SecretKey <value>' or the Identity__Jwt__SecretKey environment variable.");
        }

        if (this.AccessTokenLifetime <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Identity:Jwt:AccessTokenLifetime must be positive.");
        }
    }
}
