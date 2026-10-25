namespace Tawqi3i.QrSanad.Common.Helpers;

using System.Security.Cryptography;
using System.Text;

public static class Generator
{
    /// <summary>
    /// Time-ordered GUID, so new rows append to the primary key index instead of splitting pages.
    /// </summary>
    public static Guid CreateV7Guid() => Guid.CreateVersion7();

    /// <summary>
    /// The same name always yields the same GUID. Used for seed data, whose keys must not
    /// change between migrations.
    /// </summary>
    public static Guid CreateDeterministicGuid(string name)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(name));
        var bytes = hash.AsSpan(0, 16).ToArray();

        // RFC 9562 version 8 (custom) and variant bits.
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x80);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes, bigEndian: true);
    }

    /// <summary>
    /// Cryptographically random, URL-safe string carrying <paramref name="byteCount"/> bytes of entropy.
    /// </summary>
    public static string RandomToken(int byteCount = 32)
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(byteCount));
    }

    public static string Sha256Hex(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    public static string NewStamp() => Guid.NewGuid().ToString("N");

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
