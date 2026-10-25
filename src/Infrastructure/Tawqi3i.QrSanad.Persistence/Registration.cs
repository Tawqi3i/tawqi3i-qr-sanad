namespace Tawqi3i.QrSanad.Persistence;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Persistence.Models;
using Tawqi3i.QrSanad.Persistence.Sql;
using Tawqi3i.QrSanad.Persistence.Sql.Providers;

public static class Registration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Persistence").Get<PersistenceSettings>()
            ?? throw new InvalidOperationException("The 'Persistence' configuration section is missing.");

        services.AddSingleton(settings);
        services.AddSingleton(settings.Sql);
        services.AddSingleton(settings.Seed);

        switch (settings.Sql.Provider)
        {
            case DatabaseProvider.SqlServer:
                services.AddProviderContext<SqlServerQrSanadContext>(settings.Sql);
                break;
            case DatabaseProvider.PostgreSql:
                services.AddProviderContext<PostgreSqlQrSanadContext>(settings.Sql);
                break;
            case DatabaseProvider.Oracle:
                services.AddProviderContext<OracleQrSanadContext>(settings.Sql);
                break;
            case DatabaseProvider.MySql:
                services.AddProviderContext<MySqlQrSanadContext>(settings.Sql);
                break;
            default:
                throw new InvalidOperationException($"Unsupported database provider '{settings.Sql.Provider}'.");
        }

        if (string.IsNullOrWhiteSpace(settings.RedisCache.DefaultConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = settings.RedisCache.DefaultConnection;
                options.InstanceName = settings.RedisCache.InstanceName;
            });
        }

        services.AddRepositories();
    }

    /// <summary>
    /// Registers the provider's context and exposes it as QrSanadSqlContext, which is all the
    /// repositories know about.
    /// </summary>
    private static void AddProviderContext<TContext>(this IServiceCollection services, SqlSettings settings)
        where TContext : QrSanadSqlContext
    {
        services.AddDbContext<TContext>(options => options.UseConfiguredProvider(settings));
        services.AddScoped<QrSanadSqlContext>(provider => provider.GetRequiredService<TContext>());
    }

    /// <summary>
    /// Repositories only, for hosts (and tests) that register QrSanadSqlContext themselves.
    /// </summary>
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBranchesRepository, BranchesRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
        services.AddScoped<ISecurityAuditLogRepository, SecurityAuditLogRepository>();
        services.AddScoped<IPasswordPolicyRepository, PasswordPolicyRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
    }
}
