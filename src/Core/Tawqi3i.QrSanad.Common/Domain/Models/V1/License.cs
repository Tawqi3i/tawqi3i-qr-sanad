namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// The installed license key. A single row with <see cref="SingletonId"/>; the key is verified
/// against the embedded public key every time it is read, so editing the row cannot extend it.
/// </summary>
public class License : AuditableEntity
{
    public static readonly Guid SingletonId = new("2f8e6c1a-9b3d-4e5f-8a7c-1d2b3c4e5f60");

    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Rotated whenever a user, device or branch is added under a limit, so that two concurrent
    /// adds cannot both take the last place (see ILicenseRepository.StageLimitClaim).
    /// </summary>
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
