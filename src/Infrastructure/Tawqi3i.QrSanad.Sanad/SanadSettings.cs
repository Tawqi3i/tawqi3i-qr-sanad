namespace Tawqi3i.QrSanad.Sanad;

/// <summary>
/// Connection to MoDEE (Sanad). The account is an IDM client plus a service user, used with the
/// OAuth 2.0 password grant to obtain the token for the e-seal service.
/// </summary>
public class SanadSettings
{
    public const string IdmClientName = "SanadIdm";
    public const string ESealClientName = "SanadESeal";

    /// <summary>
    /// Root URL of the identity provider, e.g. https://staging.jordan.gov.jo.
    /// </summary>
    public string IdmBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Root URL of the e-seal service, e.g. https://stage-eseal.modee.gov.jo.
    /// </summary>
    public string ESealBaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 100;

    /// <summary>
    /// A cached access token is renewed this long before it expires.
    /// </summary>
    public int TokenRefreshMarginSeconds { get; set; } = 60;

    public bool IsConfigured =>
        Uri.TryCreate(this.IdmBaseUrl, UriKind.Absolute, out _)
        && Uri.TryCreate(this.ESealBaseUrl, UriKind.Absolute, out _)
        && !string.IsNullOrWhiteSpace(this.ClientId)
        && !string.IsNullOrWhiteSpace(this.ClientSecret)
        && !string.IsNullOrWhiteSpace(this.Username)
        && !string.IsNullOrWhiteSpace(this.Password);
}
