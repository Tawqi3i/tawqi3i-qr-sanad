namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The SuperAdmin creates devices for any branch (BranchId required). An admin creates them in
/// their own branch only, and BranchId may be omitted.
/// </summary>
public class DeviceCreateRequest
{
    /// <summary>
    /// What the device signs in with.
    /// </summary>
    [Required(ErrorMessage = "The {0} field is required")]
    [RegularExpression(ValidationPatterns.UserName, ErrorMessage = "Username must be 3 to 64 characters: letters, digits, dot, underscore or dash")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, MinimumLength = 2, ErrorMessage = "The {0} field must be {2} to {1} characters long")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string Password { get; set; } = string.Empty;

    public Guid? BranchId { get; set; }
}
