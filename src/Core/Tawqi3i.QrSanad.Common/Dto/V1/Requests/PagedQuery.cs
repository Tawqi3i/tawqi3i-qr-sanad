namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class PagedQuery
{
    [DefaultValue(1)]
    [Range(1, 100_000, ErrorMessage = "The {0} field must be between {1} and {2}")]
    public int PageNumber { get; set; } = 1;

    [DefaultValue(20)]
    [Range(1, 100, ErrorMessage = "The {0} field must be between {1} and {2}")]
    public int PageSize { get; set; } = 20;

    public int Skip => (this.PageNumber - 1) * this.PageSize;
}
