namespace Tawqi3i.QrSanad.WebApi;

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using Serilog;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Security;
using Tawqi3i.QrSanad.Domain;
using Tawqi3i.QrSanad.Identity;
using Tawqi3i.QrSanad.Licensing;
using Tawqi3i.QrSanad.Persistence;
using Tawqi3i.QrSanad.Persistence.Models;
using Tawqi3i.QrSanad.Persistence.Sql;
using Tawqi3i.QrSanad.Sanad;
using Tawqi3i.QrSanad.WebApi.Common;
using Tawqi3i.QrSanad.WebApi.Middlewares;
using Tawqi3i.QrSanad.WebApi.Services;
using Tawqi3i.QrSanad.WebApi.Settings;

internal static class Startup
{
    public const string AuthRateLimitPolicy = "auth";

    private static readonly PathString SwaggerPath = "/swagger";

    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var securitySettings = configuration.GetSection("Security").Get<SecuritySettings>() ?? new SecuritySettings();
        var rateLimitingSettings = configuration.GetSection("RateLimiting").Get<RateLimitingSettings>() ?? new RateLimitingSettings();
        services.AddSingleton(securitySettings);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = 1024 * 1024;
        });

        builder.Host.SetupLogger(typeof(Startup).Assembly);

        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)))
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    context.ToValidationResult(context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>());
            });

        // Minimal-API writers (error middleware, rate limiter, auth handler) use the same enum format.
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        services.AddEndpointsApiExplorer();
        services.AddSwagger(typeof(Startup).Assembly);

        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, HttpCurrentUserProvider>();

        services.AddPersistenceServices(configuration);
        services.AddIdentityServices(configuration);
        services.AddDomainServices(configuration);
        services.AddSanadServices(configuration);
        services.AddLicensingServices(configuration);

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, JsonAuthorizationResultHandler>();
        services.SetupCors(securitySettings.AllowedOrigins);
        services.AddRateLimiting(rateLimitingSettings);
        services.AddHealthChecks().AddDbContextCheck<QrSanadSqlContext>();

        if (securitySettings.KnownProxies.Length > 0)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                foreach (var proxy in securitySettings.KnownProxies)
                {
                    options.KnownProxies.Add(IPAddress.Parse(proxy));
                }
            });
        }

        if (securitySettings.EnforceHttps)
        {
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
            });
        }
    }

    public static void ConfigurePipeline(this WebApplication app)
    {
        var securitySettings = app.Services.GetRequiredService<SecuritySettings>();

        if (securitySettings.KnownProxies.Length > 0)
        {
            app.UseForwardedHeaders();
        }

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseSecurityHeaders(SwaggerPath);

        if (securitySettings.EnforceHttps)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
        }

        app.UseSerilogRequestLogging();
        app.UseLocalisation();

        if (securitySettings.EnableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseRouting();
        app.UseCors(WebAppExtensions.CorsPolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health").AllowAnonymous();
    }

    /// <summary>
    /// Applies migrations (when enabled) and creates the first administrator. Fails startup
    /// rather than serving requests against a database that is not ready.
    /// </summary>
    public static async Task Bootstrap(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILogger<QrSanadSqlContext>>();

        try
        {
            var context = provider.GetRequiredService<QrSanadSqlContext>();

            await DbInitializer.Migrate(context, provider.GetRequiredService<SqlSettings>(), logger);
            var passwordPolicy = await DbInitializer.SeedPasswordPolicy(context, provider.GetRequiredService<PasswordPolicySettings>(), logger);
            await DbInitializer.SeedSuperAdmin(
                context,
                provider.GetRequiredService<AdminSeedSettings>(),
                passwordPolicy,
                provider.GetRequiredService<IPasswordService>(),
                logger);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Database initialisation failed.");
            throw;
        }

        // Outside the block above: a missing or invalid license is not a database failure.
        await LicenseInitializer.Initialize(provider, provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(LicenseInitializer)));
    }

    private static void AddRateLimiting(this IServiceCollection services, RateLimitingSettings settings)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => ToOptions(settings.Global)));

            options.AddPolicy(AuthRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => ToOptions(settings.Auth)));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
                var response = Response.Fail(ResponseMessages.TooManyRequests, HttpStatusCode.TooManyRequests);

                await context.HttpContext.Response.WriteAsJsonAsync(response.Localise(localizer), cancellationToken);
            };
        });
    }

    private static string ClientKey(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static FixedWindowRateLimiterOptions ToOptions(FixedWindowSettings settings)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds),
            QueueLimit = 0
        };
    }
}
