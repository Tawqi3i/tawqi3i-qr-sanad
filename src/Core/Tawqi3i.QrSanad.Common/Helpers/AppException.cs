namespace Tawqi3i.QrSanad.Common.Helpers;

using Tawqi3i.QrSanad.Common.Dto.V1.Responses;

/// <summary>
/// Thrown to abort a request with a specific response; ErrorHandlingMiddleware writes it out.
/// </summary>
public class AppException : Exception
{
    public AppException(Response response)
        : base(response.Message)
    {
        this.Response = response;
    }

    public Response Response { get; }
}
