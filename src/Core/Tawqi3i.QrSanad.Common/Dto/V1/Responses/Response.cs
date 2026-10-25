namespace Tawqi3i.QrSanad.Common.Dto.V1.Responses;

using System.Net;
using Tawqi3i.QrSanad.Common.Constants;

public class Response : ResponseBase
{
    public static Response Succeed(string? message = ResponseMessages.Success, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new Response
        {
            Success = true,
            Message = message,
            StatusCode = (int)statusCode
        };
    }

    public static Response Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new Response
        {
            Success = false,
            Message = message,
            StatusCode = (int)statusCode
        };
    }

    public static Response NotFound(string message) => Fail(message, HttpStatusCode.NotFound);

    public static Response Conflict(string message) => Fail(message, HttpStatusCode.Conflict);

    public static Response Unauthorized(string? message = null) => Fail(message ?? ResponseMessages.Unauthorized, HttpStatusCode.Unauthorized);

    public static Response Forbidden(string? message = null) => Fail(message ?? ResponseMessages.Forbidden, HttpStatusCode.Forbidden);
}
