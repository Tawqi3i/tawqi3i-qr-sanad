namespace Tawqi3i.QrSanad.WebApi.Common;

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Tawqi3i.QrSanad.Common.Constants;

public static class WebAppExtensions
{
    public const string CorsPolicyName = "ConfiguredOrigins";

    /// <summary>
    /// Only the listed origins may call the API from a browser. Tokens travel in the
    /// Authorization header, not cookies, so credentials are not allowed.
    /// </summary>
    public static void SetupCors(this IServiceCollection services, string[] allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete)
                    .WithHeaders("Authorization", "Content-Type", "Accept-Language")
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });
    }

    /// <summary>
    /// Console and daily rolling file. Levels can be overridden from the "Serilog" configuration
    /// section. The configuration itself is never logged: it holds secrets.
    /// </summary>
    public static void SetupLogger(this IHostBuilder hostBuilder, Assembly assembly)
    {
        hostBuilder.UseSerilog((context, logger) => logger
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", $"{assembly.GetName().Name}-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14));
    }

    public static void UseLocalisation(this IApplicationBuilder app)
    {
        string[] supportedCultures = [Languages.En, Languages.Ar];

        app.UseRequestLocalization(new RequestLocalizationOptions()
            .SetDefaultCulture(Languages.En)
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures));
    }

    /// <summary>
    /// Response headers for a JSON API. Swagger UI is excluded from the strict CSP because it
    /// needs scripts and styles.
    /// </summary>
    public static void UseSecurityHeaders(this IApplicationBuilder app, PathString swaggerPath)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Cross-Origin-Resource-Policy"] = "same-origin";

            if (!context.Request.Path.StartsWithSegments(swaggerPath))
            {
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
                headers.CacheControl = "no-store";
            }

            await next();
        });
    }
}
