namespace Tawqi3i.QrSanad.WebApi.Common;

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Helpers;

public static class HttpContextExtensions
{
    /// <summary>
    /// The registered "sub" claim (RFC 7519). Claim mapping is off, so it keeps this name.
    /// </summary>
    public const string SubjectClaim = "sub";

    public static ClientInfo ExtractClient(this HttpContext httpContext)
    {
        return new ClientInfo
        {
            IpAddress = httpContext.IpAddress(),
            UserAgent = httpContext.UserAgent(),
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        };
    }

    public static RequestUser ExtractUser(this HttpContext httpContext)
    {
        var subject = httpContext.User.FindFirst(SubjectClaim)?.Value;

        if (!Guid.TryParse(subject, out var userId))
        {
            throw new AppException(Response.Unauthorized());
        }

        return new RequestUser
        {
            Id = userId,
            IpAddress = httpContext.IpAddress(),
            UserAgent = httpContext.UserAgent(),
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        };
    }

    /// <summary>
    /// The client address. Behind a reverse proxy this is only the real client when the proxy
    /// is listed in Security:KnownProxies, so that X-Forwarded-For is honoured.
    /// </summary>
    public static string? IpAddress(this HttpContext httpContext) => httpContext.Connection.RemoteIpAddress?.ToString();

    private static string? UserAgent(this HttpContext httpContext)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        return string.IsNullOrWhiteSpace(userAgent) ? null : userAgent;
    }
}
