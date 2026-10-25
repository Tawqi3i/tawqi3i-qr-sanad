namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

public class SealedDocViewModel
{
    /// <summary>
    /// Base64 contents of the sealed file.
    /// </summary>
    public string SignedFile { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}
