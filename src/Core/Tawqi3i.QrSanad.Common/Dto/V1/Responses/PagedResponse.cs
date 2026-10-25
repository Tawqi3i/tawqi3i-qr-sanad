namespace Tawqi3i.QrSanad.Common.Dto.V1.Responses;

using System.Net;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;

public class PagedResponse<T> : ResponseBase
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int PageCount => this.PageSize == 0 ? 0 : (int)Math.Ceiling(this.TotalCount / (double)this.PageSize);

    public ICollection<T> Items { get; set; } = [];

    public static PagedResponse<T> Ok(ICollection<T> items, int totalCount, PagedQuery query)
    {
        return new PagedResponse<T>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            StatusCode = (int)HttpStatusCode.OK,
            Success = true
        };
    }
}
