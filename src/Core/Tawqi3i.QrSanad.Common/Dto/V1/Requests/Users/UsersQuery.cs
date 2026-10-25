namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;

using System.ComponentModel.DataAnnotations;
using Tawqi3i.QrSanad.Common.Enums;

public class UsersQuery : PagedQuery
{
    /// <summary>
    /// Matches username, email or display name.
    /// </summary>
    [StringLength(100, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string? Search { get; set; }

    public UserStatus? Status { get; set; }

    public UserType? Type { get; set; }

    /// <summary>
    /// SuperAdmin only; everyone else always sees their own branch.
    /// </summary>
    public Guid? BranchId { get; set; }
}
