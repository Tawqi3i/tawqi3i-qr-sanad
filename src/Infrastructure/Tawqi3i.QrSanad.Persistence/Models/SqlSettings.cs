namespace Tawqi3i.QrSanad.Persistence.Models;

public class SqlSettings
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    public string DefaultConnection { get; set; } = string.Empty;

    /// <summary>
    /// Applies pending migrations at startup. Convenient in development; in production prefer
    /// running the generated SQL scripts (Database/) as a deliberate deployment step.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; }

    /// <summary>
    /// Retries transient failures (SQL Server and PostgreSQL only).
    /// </summary>
    public bool EnableRetryOnFailure { get; set; }

    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Oracle only: 19, 21 or 23. Leave empty for the provider default.
    /// </summary>
    public string? OracleSqlCompatibility { get; set; }
}
