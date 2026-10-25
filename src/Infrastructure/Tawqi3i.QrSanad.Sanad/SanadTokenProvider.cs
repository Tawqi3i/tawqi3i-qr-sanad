namespace Tawqi3i.QrSanad.Sanad;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>
/// Obtains and caches the MoDEE IDM access token (OAuth 2.0 password grant). Registered as a
/// singleton so every request shares one token until it is about to expire.
/// </summary>
internal sealed class SanadTokenProvider
{
    private const string TokenPath = "mga/sps/oauth/oauth20/token";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly SanadSettings settings;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<SanadTokenProvider> logger;
    private readonly SemaphoreSlim gate = new(1, 1);

    private string? accessToken;
    private DateTimeOffset expiresAt;

    public SanadTokenProvider(
        IHttpClientFactory httpClientFactory,
        SanadSettings settings,
        TimeProvider timeProvider,
        ILogger<SanadTokenProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.settings = settings;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    /// <summary>
    /// A valid access token, or null when IDM rejected the credentials or could not be reached.
    /// Pass the token that was just rejected as <paramref name="rejectedToken"/> to force a new one.
    /// </summary>
    public async Task<string?> GetAccessToken(CancellationToken cancellationToken, string? rejectedToken = null)
    {
        if (this.TryGetCached(rejectedToken, out var cached))
        {
            return cached;
        }

        await this.gate.WaitAsync(cancellationToken);
        try
        {
            // Another request may have renewed it while this one waited.
            if (this.TryGetCached(rejectedToken, out cached))
            {
                return cached;
            }

            var token = await this.RequestToken(cancellationToken);
            if (token == null)
            {
                return null;
            }

            var lifetime = TimeSpan.FromSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : 300);
            var margin = TimeSpan.FromSeconds(Math.Min(this.settings.TokenRefreshMarginSeconds, lifetime.TotalSeconds / 2));

            this.accessToken = token.AccessToken;
            this.expiresAt = this.timeProvider.GetUtcNow() + lifetime - margin;

            return this.accessToken;
        }
        finally
        {
            this.gate.Release();
        }
    }

    private bool TryGetCached(string? rejectedToken, out string? token)
    {
        token = this.accessToken;

        return token != null
            && token != rejectedToken
            && this.timeProvider.GetUtcNow() < this.expiresAt;
    }

    private async Task<TokenResponse?> RequestToken(CancellationToken cancellationToken)
    {
        var client = this.httpClientFactory.CreateClient(SanadSettings.IdmClientName);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = this.settings.ClientId,
            ["client_secret"] = this.settings.ClientSecret,
            ["username"] = this.settings.Username,
            ["password"] = this.settings.Password,
            ["scope"] = "openid"
        });

        using var response = await client.PostAsync(TokenPath, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            this.logger.LogError("Sanad IDM token request failed with {StatusCode}. Check the Sanad credentials in appsettings.", (int)response.StatusCode);
            return null;
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            this.logger.LogError("Sanad IDM token response did not contain an access token.");
            return null;
        }

        return token;
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
