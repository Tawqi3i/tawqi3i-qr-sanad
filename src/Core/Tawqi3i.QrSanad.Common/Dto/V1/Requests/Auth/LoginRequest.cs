namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;

using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    /// <summary>
    /// Username or email.
    /// </summary>
    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(256, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string Password { get; set; } = string.Empty;
}
