namespace Tawqi3i.QrSanad.Common.Helpers;

/// <summary>
/// Usernames, emails and role names are unique case-insensitively. Their normalized form is
/// stored and indexed, so lookups behave the same whatever collation the database uses.
/// </summary>
public static class Normalizer
{
    public static string Normalize(string value) => value.Trim().ToUpperInvariant();

    public static string? Truncate(string? value, int maxLength)
    {
        return value is null || value.Length <= maxLength ? value : value[..maxLength];
    }
}
