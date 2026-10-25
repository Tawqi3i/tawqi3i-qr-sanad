namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;
using Tawqi3i.QrSanad.Persistence.Models;

/// <summary>
/// Configures the provider and its naming convention: SQL Server keeps PascalCase, PostgreSQL
/// and MySQL use snake_case, Oracle uses UPPER_SNAKE_CASE. Migrations live in this assembly,
/// one folder per provider, bound to the provider's context in Sql/Providers.
/// </summary>
public static class DatabaseProviderExtensions
{
    public static DbContextOptionsBuilder UseConfiguredProvider(this DbContextOptionsBuilder options, SqlSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DefaultConnection))
        {
            throw new InvalidOperationException("Persistence:Sql:DefaultConnection is not configured.");
        }

        return settings.Provider switch
        {
            DatabaseProvider.SqlServer => options
                .UseSqlServer(settings.DefaultConnection, sql =>
                {
                    sql.CommandTimeout(settings.CommandTimeoutSeconds);

                    if (settings.EnableRetryOnFailure)
                    {
                        sql.EnableRetryOnFailure();
                    }
                }),
            DatabaseProvider.PostgreSql => options
                .UseNpgsql(settings.DefaultConnection, npgsql =>
                {
                    npgsql.CommandTimeout(settings.CommandTimeoutSeconds);

                    if (settings.EnableRetryOnFailure)
                    {
                        npgsql.EnableRetryOnFailure();
                    }
                })
                .UseSnakeCaseNamingConvention(),
            DatabaseProvider.Oracle => options
                .UseOracle(settings.DefaultConnection, oracle =>
                {
                    oracle.CommandTimeout(settings.CommandTimeoutSeconds);

                    var compatibility = ToOracleSqlCompatibility(settings.OracleSqlCompatibility);

                    if (compatibility != null)
                    {
                        oracle.UseOracleSQLCompatibility(compatibility.Value);
                    }
                })
                .UseUpperSnakeCaseNamingConvention(),
            DatabaseProvider.MySql => options
                .UseMySQL(settings.DefaultConnection, mysql =>
                {
                    mysql.CommandTimeout(settings.CommandTimeoutSeconds);
                })
                .UseSnakeCaseNamingConvention(),
            _ => throw new ArgumentOutOfRangeException(nameof(settings), settings.Provider, "Unsupported database provider.")
        };
    }

    public static DbContextOptionsBuilder<TContext> UseConfiguredProvider<TContext>(this DbContextOptionsBuilder<TContext> options, SqlSettings settings)
        where TContext : DbContext
    {
        ((DbContextOptionsBuilder)options).UseConfiguredProvider(settings);
        return options;
    }

    private static OracleSQLCompatibility? ToOracleSqlCompatibility(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return null;
        }

        return level.Trim().ToLowerInvariant() switch
        {
            "19" or "19c" => OracleSQLCompatibility.DatabaseVersion19,
            "21" or "21c" => OracleSQLCompatibility.DatabaseVersion21,
            "23" or "23ai" or "26" or "26ai" => OracleSQLCompatibility.DatabaseVersion23,
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unsupported Oracle SQL compatibility level. Use 19, 21 or 23.")
        };
    }
}
