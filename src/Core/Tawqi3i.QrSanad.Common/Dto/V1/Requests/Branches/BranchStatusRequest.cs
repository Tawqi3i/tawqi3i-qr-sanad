namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;

using System.ComponentModel.DataAnnotations;
using Tawqi3i.QrSanad.Common.Enums;

public class BranchStatusRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [EnumDataType(typeof(BranchStatus), ErrorMessage = "The {0} field is invalid")]
    public BranchStatus Status { get; set; }
}
