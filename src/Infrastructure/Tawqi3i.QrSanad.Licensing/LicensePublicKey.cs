namespace Tawqi3i.QrSanad.Licensing;

/// <summary>
/// The ECDSA P-256 public key (SubjectPublicKeyInfo, base64) that license signatures are checked
/// against. Its private key stays with whoever issues licenses and is never committed. Generated
/// with <c>Tawqi3i.QrSanad.LicenseGenerator keygen</c>; changing it invalidates every issued license.
/// </summary>
internal static class LicensePublicKey
{
    public const string Value = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEZLp01XpR49w+7jYudY9EX4Xb+elDvZhmtKQ63RgPnxjYYk5xZiEI6UU09fCSP73AQ0Yj4/mc4BMYwxhdOMhPWA==";
}
