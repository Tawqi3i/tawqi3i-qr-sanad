namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// Only the SHA-256 hash of the token is stored, so a database leak does not leak usable tokens.
/// Tokens are single use: refreshing revokes the old one and links it to its replacement.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// The user's security stamp at issue time. A refresh is refused once the user's stamp has
    /// changed, so rotating the stamp ends refresh tokens as well as access tokens.
    /// </summary>
    public string SecurityStamp { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedByIp { get; set; }

    public string? RevokeReason { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public User User { get; set; } = null!;
}
