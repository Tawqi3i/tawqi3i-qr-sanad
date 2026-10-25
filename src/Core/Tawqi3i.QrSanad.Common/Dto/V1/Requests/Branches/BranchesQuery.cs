namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;

using System.ComponentModel.DataAnnotations;
using Tawqi3i.QrSanad.Common.Enums;

public class BranchesQuery : PagedQuery
{
    /// <summary>
    /// Matches name or code.
    /// </summary>
    [StringLength(100, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string? Search { get; set; }

    public BranchStatus? Status { get; set; }
}
