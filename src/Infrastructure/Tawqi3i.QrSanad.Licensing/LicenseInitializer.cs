namespace Tawqi3i.QrSanad.Licensing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tawqi3i.QrSanad.Common.Persistence;

/// <summary>
/// Runs at startup. Like TrustedSigning, the API refuses to start without a valid license; an
/// expired one still starts, so the SuperAdmin can sign in and install a renewal.
/// </summary>
public static class LicenseInitializer
{
    public static async Task<LicenseData> Initialize(IServiceProvider provider, ILogger logger, CancellationToken cancellationToken = default)
    {
        var verifier = provider.GetRequiredService<LicenseVerifier>();
        var settings = provider.GetRequiredService<LicensingSettings>();
        var repository = provider.GetRequiredService<ILicenseRepository>();
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var timeProvider = provider.GetRequiredService<TimeProvider>();

        LicenseData? configured = null;

        if (!string.IsNullOrWhiteSpace(settings.Key))
        {
            configured = verifier.Verify(settings.Key)
                ?? throw new InvalidOperationException("Licensing:Key is not a valid QrSanad license key.");
        }

        var storedKey = await repository.GetStoredKey(cancellationToken);
        var stored = verifier.Verify(storedKey);

        if (storedKey != null && stored == null)
        {
            logger.LogWarning("The stored license key is not valid and was ignored.");
        }

        var license = stored;

        if (ReplacesStored(configured, stored))
        {
            await repository.StageKey(settings.Key.Trim(), cancellationToken);
            await unitOfWork.SaveChanges(cancellationToken);
            license = configured;
        }

        if (license == null)
        {
            throw new InvalidOperationException(
                "No license is installed. Set Licensing:Key to a key issued with Tawqi3i.QrSanad.LicenseGenerator.");
        }

        if (license.IsExpired(timeProvider.GetUtcNow().UtcDateTime))
        {
            logger.LogWarning("The license has expired: {License}. Scans and new users and branches are blocked until it is renewed.", license);
        }
        else
        {
            logger.LogInformation("Licensed to {License}.", license);
        }

        return license;
    }

    /// <summary>
    /// A license can arrive through configuration as well as the API, and the most recently
    /// issued one wins, even when it grants less: a new key always replaces the old one, while
    /// an older key left in configuration does not undo a newer one installed through the API.
    /// </summary>
    internal static bool ReplacesStored(LicenseData? configured, LicenseData? stored) =>
        configured != null && (stored == null || configured.IssuedAt > stored.IssuedAt);
}
