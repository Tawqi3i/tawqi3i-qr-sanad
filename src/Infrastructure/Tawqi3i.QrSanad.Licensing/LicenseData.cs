namespace Tawqi3i.QrSanad.Licensing;

/// <summary>
/// What a license grants. Signed by the license generator and verified by the API.
/// </summary>
public sealed record LicenseData
{
    public string Customer { get; init; } = string.Empty;

    /// <summary>
    /// TRIAL, PROD or DEV.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    public DateTime IssuedAt { get; init; }

    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Admins and users allowed, not counting the SuperAdmin. Null means unlimited.
    /// </summary>
    public int? MaxUsers { get; init; }

    /// <summary>
    /// Null means unlimited.
    /// </summary>
    public int? MaxBranches { get; init; }

    public bool IsExpired(DateTime utcNow) => utcNow >= this.ExpiresAt;

    /// <summary>
    /// A one-line summary, for logs and the audit log.
    /// </summary>
    public override string ToString() =>
        $"{this.Customer} ({this.Type}), until {this.ExpiresAt:yyyy-MM-dd}, users {this.MaxUsers?.ToString() ?? "unlimited"}, " +
        $"branches {this.MaxBranches?.ToString() ?? "unlimited"}";
}
