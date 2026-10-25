namespace Tawqi3i.QrSanad.Sanad;

using System.Text.Json;

/// <summary>
/// Reads the MoDEE SealedDoc response. The result may be wrapped in a <c>data</c> array, and the
/// document sits in <c>data.signedFile.fileContents</c>; errors carry <c>status</c> and <c>message</c>.
/// </summary>
internal static class SealedDocParser
{
    public const string DefaultContentType = "application/pdf";

    /// <summary>
    /// The parsed result, or null when the body is not JSON.
    /// </summary>
    public static SealedDocResult? Parse(string body, int httpStatus)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            return null;
        }

        using (document)
        {
            var item = document.RootElement;

            if (item.ValueKind == JsonValueKind.Object
                && item.TryGetProperty("data", out var outerData)
                && outerData.ValueKind == JsonValueKind.Array)
            {
                item = outerData.GetArrayLength() > 0 ? outerData[0] : default;
            }

            string? fileContents = null;
            string? contentType = null;

            if (item.ValueKind == JsonValueKind.Object
                && item.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("signedFile", out var signedFile)
                && signedFile.ValueKind == JsonValueKind.Object)
            {
                fileContents = GetString(signedFile, "fileContents");
                contentType = GetString(signedFile, "contentType");
            }

            return new SealedDocResult(
                string.IsNullOrWhiteSpace(fileContents) ? null : fileContents,
                contentType ?? DefaultContentType,
                GetInt(item, "status") ?? httpStatus,
                GetString(item, "message"));
        }
    }

    private static string? GetString(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    private static int? GetInt(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
                ? number
                : null;
    }
}

/// <param name="FileContents">Base64 document, or null when MoDEE returned none.</param>
/// <param name="Status">MoDEE's own status when present, otherwise the HTTP status.</param>
internal sealed record SealedDocResult(string? FileContents, string ContentType, int Status, string? Message);
