namespace Tawqi3i.QrSanad.Identity;

using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Helpers;

public interface ITokenProvider
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user, DateTime now);

    /// <summary>
    /// A new opaque refresh token. Only <c>Hash</c> is stored; <c>Token</c> goes to the client.
    /// </summary>
    (string Token, string Hash) CreateRefreshToken();

    string HashRefreshToken(string token);
}

internal sealed class JwtProvider : ITokenProvider
{
    private readonly JwtSettings settings;
    private readonly SigningCredentials signingCredentials;
    private readonly JsonWebTokenHandler handler = new();

    public JwtProvider(JwtSettings settings)
    {
        this.settings = settings;
        this.signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            SecurityAlgorithms.HmacSha256);
    }

    // Roles and permissions are deliberately not claims: they are resolved per request from
    // IUserAccessProvider, so revoking access works without waiting for the token to expire.
    public (string Token, DateTime ExpiresAt) CreateAccessToken(User user, DateTime now)
    {
        var expiresAt = now.Add(this.settings.AccessTokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(CustomClaimNames.SecurityStamp, user.SecurityStamp),
        };

        if (user.MustChangePassword)
        {
            claims.Add(new Claim(CustomClaimNames.PasswordChangeRequired, "true"));
        }

        var token = this.handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = this.settings.Issuer,
            Audience = this.settings.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = this.signingCredentials
        });

        return (token, expiresAt);
    }

    public (string Token, string Hash) CreateRefreshToken()
    {
        var token = Generator.RandomToken(64);
        return (token, this.HashRefreshToken(token));
    }

    // A plain SHA-256 is enough here: the token carries 512 bits of entropy, so unlike a
    // password it cannot be brute-forced from its hash.
    public string HashRefreshToken(string token) => Generator.Sha256Hex(token);
}
