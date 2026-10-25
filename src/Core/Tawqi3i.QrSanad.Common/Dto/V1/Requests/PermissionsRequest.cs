namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The complete set of permission codes a user or role should end up with.
/// </summary>
public class PermissionsRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [MaxLength(200, ErrorMessage = "The {0} field must not contain more than {1} items")]
    public ICollection<string> Permissions { get; set; } = [];
}
