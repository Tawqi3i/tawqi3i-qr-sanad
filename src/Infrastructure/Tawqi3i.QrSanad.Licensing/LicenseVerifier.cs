namespace Tawqi3i.QrSanad.Licensing;

using System.Security.Cryptography;
using System.Text.Json;

/// <summary>
/// Checks a license key's signature against a public key and reads its contents. Expiry is
/// not checked here; callers decide what an expired license still allows.
/// </summary>
public sealed class LicenseVerifier
{
    private readonly byte[] subjectPublicKeyInfo;

    public LicenseVerifier(byte[] subjectPublicKeyInfo)
    {
        this.subjectPublicKeyInfo = subjectPublicKeyInfo;
    }

    /// <summary>
    /// Verifies against the public key built into the API, matching the private key the license
    /// generator signs with.
    /// </summary>
    public static LicenseVerifier CreateDefault() => new(Convert.FromBase64String(LicensePublicKey.Value));

    /// <summary>
    /// The license, or null when the key is malformed or its signature does not match.
    /// </summary>
    public LicenseData? Verify(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || !LicenseKeyFormat.TryDecode(key.Trim(), out var payload, out var signature))
        {
            return null;
        }

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(this.subjectPublicKeyInfo, out _);

        if (!ecdsa.VerifyData(payload, signature, HashAlgorithmName.SHA256))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LicenseData>(payload, LicenseKeyFormat.JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
