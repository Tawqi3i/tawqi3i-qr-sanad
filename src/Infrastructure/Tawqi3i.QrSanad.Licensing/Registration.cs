namespace Tawqi3i.QrSanad.Licensing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawqi3i.QrSanad.Common.Licensing;

public static class Registration
{
    public static void AddLicensingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection("Licensing").Get<LicensingSettings>() ?? new LicensingSettings());
        services.AddSingleton(LicenseVerifier.CreateDefault());
        services.AddScoped<ILicenseService, LicenseService>();
    }
}
