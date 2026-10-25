namespace Tawqi3i.QrSanad.Persistence.Sql;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Tawqi3i.QrSanad.Common.Persistence;

/// <summary>
/// Maps each provider's constraint-violation error to one provider-neutral code, so callers
/// handle "duplicate key" the same way whichever database is configured.
/// </summary>
internal static class PersistenceExceptionTranslator
{
    // SQL Server: https://learn.microsoft.com/sql/relational-databases/errors-events/database-engine-events-and-errors
    private const int SqlServerUniqueConstraint = 2627;
    private const int SqlServerUniqueIndex = 2601;
    private const int SqlServerForeignKey = 547;

    // PostgreSQL: https://www.postgresql.org/docs/current/errcodes-appendix.html
    private const string PostgresUniqueViolation = "23505";
    private const string PostgresForeignKeyViolation = "23503";

    // Oracle: ORA-00001, ORA-02291, ORA-02292
    private const int OracleUniqueViolation = 1;
    private const int OracleParentKeyNotFound = 2291;
    private const int OracleChildRecordFound = 2292;

    // MySQL: https://dev.mysql.com/doc/mysql-errors/8.0/en/server-error-reference.html
    private const int MySqlDuplicateEntry = 1062;
    private const int MySqlRowIsReferenced = 1451;
    private const int MySqlNoReferencedRow = 1452;

    public static bool TryTranslate(Exception exception, [NotNullWhen(true)] out PersistenceException? translated)
    {
        if (exception is DbUpdateConcurrencyException)
        {
            translated = new PersistenceException(exception.Message, ErrorCodes.ConcurrencyConflict, exception);
            return true;
        }

        var code = exception.InnerException switch
        {
            SqlException sql => sql.Number switch
            {
                SqlServerUniqueConstraint or SqlServerUniqueIndex => ErrorCodes.DuplicateKey,
                SqlServerForeignKey => ErrorCodes.InvalidForeignKey,
                _ => null
            },
            PostgresException postgres => postgres.SqlState switch
            {
                PostgresUniqueViolation => ErrorCodes.DuplicateKey,
                PostgresForeignKeyViolation => ErrorCodes.InvalidForeignKey,
                _ => null
            },
            OracleException oracle => oracle.Number switch
            {
                OracleUniqueViolation => ErrorCodes.DuplicateKey,
                OracleParentKeyNotFound or OracleChildRecordFound => ErrorCodes.InvalidForeignKey,
                _ => null
            },
            MySqlException mysql => mysql.Number switch
            {
                MySqlDuplicateEntry => ErrorCodes.DuplicateKey,
                MySqlRowIsReferenced or MySqlNoReferencedRow => ErrorCodes.InvalidForeignKey,
                _ => null
            },
            _ => null
        };

        translated = code == null ? null : new PersistenceException(exception.InnerException!.Message, code, exception);
        return translated != null;
    }
}
