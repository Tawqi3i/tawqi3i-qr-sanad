namespace Tawqi3i.QrSanad.Common.Dto.V1.Responses;

public class TokenResponse
{
    public string TokenType { get; init; } = "Bearer";

    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// When true, only change-password, profile and logout accept this token.
    /// </summary>
    public bool MustChangePassword { get; init; }
}
