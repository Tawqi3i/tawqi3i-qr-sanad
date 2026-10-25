namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;

using System.ComponentModel.DataAnnotations;
using Tawqi3i.QrSanad.Common.Enums;

public class UserStatusRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [EnumDataType(typeof(UserStatus), ErrorMessage = "The {0} field is invalid")]
    public UserStatus Status { get; set; }
}
