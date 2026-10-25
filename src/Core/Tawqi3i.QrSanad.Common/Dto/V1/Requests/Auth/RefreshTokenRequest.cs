namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;

using System.ComponentModel.DataAnnotations;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(256, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string RefreshToken { get; set; } = string.Empty;
}
