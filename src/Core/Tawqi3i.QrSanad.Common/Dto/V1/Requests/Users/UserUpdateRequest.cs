namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;

using System.ComponentModel.DataAnnotations;

public class UserUpdateRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid email address")]
    [StringLength(256, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, MinimumLength = 2, ErrorMessage = "The {0} field must be {2} to {1} characters long")]
    public string DisplayName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "The {0} field is not a valid phone number")]
    [StringLength(32, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string? PhoneNumber { get; set; }
}
