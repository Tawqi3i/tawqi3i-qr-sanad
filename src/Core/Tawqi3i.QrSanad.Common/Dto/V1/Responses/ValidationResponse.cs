namespace Tawqi3i.QrSanad.Common.Dto.V1.Responses;

public class ValidationResponse : ResponseBase
{
    /// <summary>
    /// Field name to its error messages.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}
