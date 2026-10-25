namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests;

public static class ValidationPatterns
{
    /// <summary>
    /// No '@', so a login identifier is unambiguously either a username or an email.
    /// </summary>
    public const string UserName = "^[A-Za-z0-9._-]{3,64}$";
}
