namespace Tawqi3i.QrSanad.WebApi.Common;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";

    public static void AddSwagger(this IServiceCollection services, Assembly assembly)
    {
        services.AddSwaggerGen(options =>
        {
            var name = assembly.GetName().Name;
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{name}.xml");

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.SwaggerDoc("v1", new OpenApiInfo { Title = name, Version = "v1" });

            options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "The accessToken returned by POST /api/v1/auth/login"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerScheme, document)] = []
            });
        });
    }
}
