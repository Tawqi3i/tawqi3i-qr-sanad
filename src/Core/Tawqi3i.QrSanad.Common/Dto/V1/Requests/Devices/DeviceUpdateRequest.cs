namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;

using System.ComponentModel.DataAnnotations;

public class DeviceUpdateRequest
{
    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(128, MinimumLength = 2, ErrorMessage = "The {0} field must be {2} to {1} characters long")]
    public string DisplayName { get; set; } = string.Empty;
}
