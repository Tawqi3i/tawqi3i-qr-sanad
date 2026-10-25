namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Creates or updates a branch.
/// </summary>
public class BranchRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, MinimumLength = 2, ErrorMessage = "The {0} field must be {2} to {1} characters long")]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^[A-Za-z0-9_-]{1,32}$", ErrorMessage = "The {0} field is invalid")]
    public string? Code { get; set; }

    [StringLength(256, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "The {0} field is not a valid phone number")]
    [StringLength(32, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string? PhoneNumber { get; set; }
}
