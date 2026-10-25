namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;

using System.ComponentModel.DataAnnotations;
using Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// The SuperAdmin creates admins and users for any branch (BranchId required). An admin creates
/// users in their own branch only: Type must be User and BranchId may be omitted.
/// </summary>
public class UserCreateRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [RegularExpression(ValidationPatterns.UserName, ErrorMessage = "Username must be 3 to 64 characters: letters, digits, dot, underscore or dash")]
    public string UserName { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Admin or User. Defaults to User.
    /// </summary>
    [EnumDataType(typeof(UserType), ErrorMessage = "The {0} field is invalid")]
    public UserType Type { get; set; } = UserType.User;

    public Guid? BranchId { get; set; }

    /// <summary>
    /// Forces the user to choose their own password on first login. Defaults to true.
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    /// <summary>
    /// Permission codes to grant. When creating an admin and this is left empty, the admin gets
    /// every permission.
    /// </summary>
    [MaxLength(200, ErrorMessage = "The {0} field must not contain more than {1} items")]
    public ICollection<string> Permissions { get; set; } = [];
}
