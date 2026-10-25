namespace Tawqi3i.QrSanad.Common.Dto.V1.Responses;

using System.Text.Json.Serialization;

public abstract class ResponseBase
{
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonIgnore]
    public bool Success { get; set; }

    [JsonIgnore]
    public int? StatusCode { get; set; }
}
