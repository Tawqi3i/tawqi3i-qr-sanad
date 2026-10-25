namespace Tawqi3i.QrSanad.Common.Dto.V1.Responses;

using System.Net;
using System.Text.Json.Serialization;
using Tawqi3i.QrSanad.Common.Constants;

public class Response<T> : ResponseBase
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public static Response<T> Succeed(T data, string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new Response<T>
        {
            Success = true,
            Message = message,
            StatusCode = (int)statusCode,
            Data = data
        };
    }

    public static Response<T> Created(T data) => Succeed(data, ResponseMessages.Created, HttpStatusCode.Created);

    public static Response<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new Response<T>
        {
            Success = false,
            Message = message,
            StatusCode = (int)statusCode
        };
    }

    public static Response<T> NotFound(string message) => Fail(message, HttpStatusCode.NotFound);

    public static Response<T> Conflict(string message) => Fail(message, HttpStatusCode.Conflict);

    public static Response<T> Unauthorized(string? message = null) => Fail(message ?? ResponseMessages.Unauthorized, HttpStatusCode.Unauthorized);

    public static Response<T> Forbidden(string? message = null) => Fail(message ?? ResponseMessages.Forbidden, HttpStatusCode.Forbidden);

    /// <summary>
    /// Carries a failed non-generic response into a generic one.
    /// </summary>
    public static Response<T> From(Response failure)
    {
        return new Response<T>
        {
            Success = failure.Success,
            Message = failure.Message,
            StatusCode = failure.StatusCode
        };
    }
}
