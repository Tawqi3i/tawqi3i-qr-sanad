namespace Tawqi3i.QrSanad.Persistence.Sql.Providers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tawqi3i.QrSanad.Persistence.Models;

// Used only by `dotnet ef` (pick one with --context). `migrations add` and `migrations script`
// never open a connection; `database update` does, so point it at a real database with the
// QRSANAD_CONNECTION environment variable.

internal sealed class SqlServerDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerQrSanadContext>
{
    public SqlServerQrSanadContext CreateDbContext(string[] args) => new(DesignTime.Options<SqlServerQrSanadContext>(
        DatabaseProvider.SqlServer,
        "Server=(localdb)\\MSSQLLocalDB;Database=QrSanadDb;Trusted_Connection=True;TrustServerCertificate=True"));
}

internal sealed class PostgreSqlDesignTimeFactory : IDesignTimeDbContextFactory<PostgreSqlQrSanadContext>
{
    public PostgreSqlQrSanadContext CreateDbContext(string[] args) => new(DesignTime.Options<PostgreSqlQrSanadContext>(
        DatabaseProvider.PostgreSql,
        "Host=localhost;Port=5432;Database=qrsanad;Username=postgres"));
}

internal sealed class OracleDesignTimeFactory : IDesignTimeDbContextFactory<OracleQrSanadContext>
{
    public OracleQrSanadContext CreateDbContext(string[] args) => new(DesignTime.Options<OracleQrSanadContext>(
        DatabaseProvider.Oracle,
        "User Id=QRSANAD;Data Source=localhost:1521/FREEPDB1"));
}

internal sealed class MySqlDesignTimeFactory : IDesignTimeDbContextFactory<MySqlQrSanadContext>
{
    public MySqlQrSanadContext CreateDbContext(string[] args) => new(DesignTime.Options<MySqlQrSanadContext>(
        DatabaseProvider.MySql,
        "Server=localhost;Port=3306;Database=qrsanad;User=root"));
}

internal static class DesignTime
{
    public static DbContextOptions<TContext> Options<TContext>(DatabaseProvider provider, string defaultConnection)
        where TContext : DbContext
    {
        var settings = new SqlSettings
        {
            Provider = provider,
            DefaultConnection = Environment.GetEnvironmentVariable("QRSANAD_CONNECTION") ?? defaultConnection
        };

        return new DbContextOptionsBuilder<TContext>().UseConfiguredProvider(settings).Options;
    }
}
