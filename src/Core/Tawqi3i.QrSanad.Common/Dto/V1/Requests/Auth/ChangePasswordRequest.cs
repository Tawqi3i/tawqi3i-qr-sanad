namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;

using System.ComponentModel.DataAnnotations;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "The {0} field is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
