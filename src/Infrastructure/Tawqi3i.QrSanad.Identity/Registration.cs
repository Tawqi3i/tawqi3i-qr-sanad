namespace Tawqi3i.QrSanad.Identity;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Security;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.Identity.Services;

public static class Registration
{
    public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Identity:Jwt").Get<JwtSettings>() ?? new JwtSettings();
        jwtSettings.Validate();

        var authSettings = configuration.GetSection("Identity:Auth").Get<AuthSettings>() ?? new AuthSettings();
        var passwordPolicy = configuration.GetSection("Identity:PasswordPolicy").Get<PasswordPolicySettings>() ?? new PasswordPolicySettings();

        services.AddSingleton(jwtSettings);
        services.AddSingleton(authSettings);
        services.AddSingleton(passwordPolicy);

        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenProvider, JwtProvider>();
        services.AddScoped<IUserAccessProvider, UserAccessProvider>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.UniqueName
                };

                // A valid signature is not enough: the user must still exist, be active and
                // carry the same security stamp the token was issued with.
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        var subject = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                        var stamp = principal?.FindFirst(CustomClaimNames.SecurityStamp)?.Value;

                        if (!Guid.TryParse(subject, out var userId) || string.IsNullOrEmpty(stamp))
                        {
                            context.Fail("The token is missing required claims.");
                            return;
                        }

                        var accessProvider = context.HttpContext.RequestServices.GetRequiredService<IUserAccessProvider>();
                        var access = await accessProvider.Get(userId, context.HttpContext.RequestAborted);

                        if (access is not { IsActive: true } || !string.Equals(access.SecurityStamp, stamp, StringComparison.Ordinal))
                        {
                            context.Fail("The token is no longer valid.");
                        }
                    }
                };
            });

        // Secure by default: every endpoint requires an authenticated user unless it opts out
        // with [AllowAnonymous].
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
    }
}
