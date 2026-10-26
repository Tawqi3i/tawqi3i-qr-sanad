namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;

using System.ComponentModel.DataAnnotations;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;

public class ScansQuery : AuditLogsFilter
{
    /// <summary>
    /// Digits search the national number from its start; anything else searches the Arabic and
    /// English names.
    /// </summary>
    [StringLength(256, ErrorMessage = "The {0} field must not exceed {1} characters")]
    public string? Search { get; set; }
}
