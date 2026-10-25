namespace Tawqi3i.QrSanad.Licensing;

using System.Buffers.Text;
using System.Text.Json;

/// <summary>
/// A license key is <c>QRS-LIC-{payload}.{signature}</c>: the JSON <see cref="LicenseData"/> and
/// its ECDSA P-256 / SHA-256 signature, both base64url. The payload is readable; only the
/// signature makes it trustworthy.
/// </summary>
public static class LicenseKeyFormat
{
    public const string Prefix = "QRS-LIC-";

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static byte[] Serialize(LicenseData license) => JsonSerializer.SerializeToUtf8Bytes(license, JsonOptions);

    public static string Encode(byte[] payload, byte[] signature) =>
        Prefix + Base64Url.EncodeToString(payload) + "." + Base64Url.EncodeToString(signature);

    public static bool TryDecode(string key, out byte[] payload, out byte[] signature)
    {
        payload = [];
        signature = [];

        if (!key.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = key[Prefix.Length..].Split('.');

        if (parts.Length != 2 || !Base64Url.IsValid(parts[0]) || !Base64Url.IsValid(parts[1]))
        {
            return false;
        }

        payload = Base64Url.DecodeFromChars(parts[0]);
        signature = Base64Url.DecodeFromChars(parts[1]);

        return payload.Length > 0 && signature.Length > 0;
    }
}
