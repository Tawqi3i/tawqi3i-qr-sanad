namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests;

/// <summary>
/// Who is calling, as far as the transport can tell. Extracted from HttpContext by the controllers.
/// </summary>
public class ClientInfo
{
    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string? Language { get; init; }
}

/// <summary>
/// An authenticated caller: the subject of the access token plus their client details.
/// </summary>
public class RequestUser : ClientInfo
{
    public Guid Id { get; init; }
}
