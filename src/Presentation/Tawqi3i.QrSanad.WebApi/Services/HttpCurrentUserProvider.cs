namespace Tawqi3i.QrSanad.WebApi.Services;

using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.WebApi.Common;

internal sealed class HttpCurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var subject = this.httpContextAccessor.HttpContext?.User.FindFirst(HttpContextExtensions.SubjectClaim)?.Value;
            return Guid.TryParse(subject, out var userId) ? userId : null;
        }
    }
}
