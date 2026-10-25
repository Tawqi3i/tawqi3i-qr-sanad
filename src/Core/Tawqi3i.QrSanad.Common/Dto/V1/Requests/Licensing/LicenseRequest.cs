namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Licensing;

using System.ComponentModel.DataAnnotations;

public class LicenseRequest
{
    /// <summary>
    /// Keys are a few hundred characters. 2000 keeps the column an ordinary string on every
    /// provider (Oracle NVARCHAR2 tops out at 2000; beyond it becomes a LOB).
    /// </summary>
    public const int MaxKeyLength = 2000;

    [Required(ErrorMessage = "The {0} field is required")]
    [StringLength(MaxKeyLength, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string Key { get; set; } = string.Empty;
}
