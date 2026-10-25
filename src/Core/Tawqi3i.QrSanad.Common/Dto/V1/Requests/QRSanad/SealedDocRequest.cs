namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;

using System.ComponentModel.DataAnnotations;

public class SealedDocRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(4096, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string QrCode { get; set; } = string.Empty;
}
