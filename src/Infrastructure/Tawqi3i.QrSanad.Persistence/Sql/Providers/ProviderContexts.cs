namespace Tawqi3i.QrSanad.Persistence.Sql.Providers;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Identity;

// One subclass per provider, each owning the migrations in Migrations/<Provider>.
// They add nothing to the model: the generated SQL differs only through the provider and
// naming convention that DatabaseProviderExtensions configures.

public sealed class SqlServerQrSanadContext(
    DbContextOptions<SqlServerQrSanadContext> options,
    ICurrentUserProvider? currentUserProvider = null,
    TimeProvider? timeProvider = null)
    : QrSanadSqlContext(options, currentUserProvider, timeProvider);

public sealed class PostgreSqlQrSanadContext(
    DbContextOptions<PostgreSqlQrSanadContext> options,
    ICurrentUserProvider? currentUserProvider = null,
    TimeProvider? timeProvider = null)
    : QrSanadSqlContext(options, currentUserProvider, timeProvider);

public sealed class OracleQrSanadContext(
    DbContextOptions<OracleQrSanadContext> options,
    ICurrentUserProvider? currentUserProvider = null,
    TimeProvider? timeProvider = null)
    : QrSanadSqlContext(options, currentUserProvider, timeProvider);

public sealed class MySqlQrSanadContext(
    DbContextOptions<MySqlQrSanadContext> options,
    ICurrentUserProvider? currentUserProvider = null,
    TimeProvider? timeProvider = null)
    : QrSanadSqlContext(options, currentUserProvider, timeProvider);
