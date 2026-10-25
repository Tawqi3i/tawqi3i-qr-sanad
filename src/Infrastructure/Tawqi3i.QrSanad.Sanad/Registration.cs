namespace Tawqi3i.QrSanad.Sanad;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawqi3i.QrSanad.Common.Sanad;

public static class Registration
{
    /// <summary>
    /// Registers the MoDEE (Sanad) client. Missing settings do not stop the host from starting;
    /// calls fail with 502 and an error is logged instead.
    /// </summary>
    public static void AddSanadServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Sanad").Get<SanadSettings>() ?? new SanadSettings();

        services.AddSingleton(settings);
        services.AddHttpClient(SanadSettings.IdmClientName, client => Configure(client, settings.IdmBaseUrl, settings));
        services.AddHttpClient(SanadSettings.ESealClientName, client => Configure(client, settings.ESealBaseUrl, settings));

        services.AddSingleton<SanadTokenProvider>();
        services.AddScoped<IQRSanadClient, SanadClient>();
    }

    private static void Configure(HttpClient client, string baseUrl, SanadSettings settings)
    {
        if (Uri.TryCreate(baseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseAddress))
        {
            client.BaseAddress = baseAddress;
        }

        client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }
}
