namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// The model, shared by every provider. Each provider has a thin subclass in Sql/Providers so
/// that its migrations (Migrations/&lt;Provider&gt;) are bound to it; repositories and services
/// depend only on this base type, which Registration maps to the configured subclass.
/// </summary>
public abstract class QrSanadSqlContext : DbContext
{
    private readonly ICurrentUserProvider? currentUserProvider;
    private readonly TimeProvider timeProvider;

    protected QrSanadSqlContext(
        DbContextOptions options,
        ICurrentUserProvider? currentUserProvider,
        TimeProvider? timeProvider)
        : base(options)
    {
        this.currentUserProvider = currentUserProvider;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public DbSet<Branch> Branches => this.Set<Branch>();

    public DbSet<User> Users => this.Set<User>();

    public DbSet<Permission> Permissions => this.Set<Permission>();

    public DbSet<UserPermission> UserPermissions => this.Set<UserPermission>();

    public DbSet<RefreshToken> RefreshTokens => this.Set<RefreshToken>();

    public DbSet<SecurityAuditLog> SecurityAuditLogs => this.Set<SecurityAuditLog>();

    public DbSet<PasswordPolicy> PasswordPolicies => this.Set<PasswordPolicy>();

    public DbSet<License> Licenses => this.Set<License>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.StampAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.StampAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QrSanadSqlContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Every timestamp is UTC. SQL Server, Oracle and MySQL return DateTimeKind.Unspecified,
        // so mark values as UTC on the way out; PostgreSQL requires UTC on the way in.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    private void StampAuditFields()
    {
        var now = this.timeProvider.GetUtcNow().UtcDateTime;
        var userId = this.currentUserProvider?.UserId;

        foreach (var entry in this.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy ??= userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId ?? entry.Entity.UpdatedBy;
                    break;
            }
        }
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        value => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value.ToUniversalTime(), DateTimeKind.Utc),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
        value => value.HasValue
            ? (value.Value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value.Value.ToUniversalTime(), DateTimeKind.Utc))
            : value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
}
