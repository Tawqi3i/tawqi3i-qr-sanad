namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Security;
using Tawqi3i.QrSanad.Persistence.Models;

public static class DbInitializer
{
    public static async Task Migrate(QrSanadSqlContext context, SqlSettings settings, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (!settings.ApplyMigrationsOnStartup)
        {
            return;
        }

        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pending.Count == 0)
        {
            return;
        }

        logger.LogInformation("Applying {Count} pending migration(s) on {Provider}: {Migrations}", pending.Count, settings.Provider, pending);
        await context.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Stores Identity:PasswordPolicy as the initial policy when none is stored yet, and returns
    /// the policy in force. From then on the SuperAdmin changes it through the API, and the
    /// configuration section is only a fallback.
    /// </summary>
    public static async Task<PasswordPolicySettings> SeedPasswordPolicy(
        QrSanadSqlContext context,
        PasswordPolicySettings defaults,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var stored = await context.PasswordPolicies.AsNoTracking().FirstOrDefaultAsync(a => a.Id == PasswordPolicy.SingletonId, cancellationToken);

        if (stored != null)
        {
            return PasswordPolicySettings.From(stored);
        }

        var policy = new PasswordPolicy { Id = PasswordPolicy.SingletonId };
        defaults.ApplyTo(policy);

        context.PasswordPolicies.Add(policy);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Stored the initial password policy from Identity:PasswordPolicy.");

        return defaults;
    }

    /// <summary>
    /// Creates the SuperAdmin when none exists yet. Permissions are seeded by the migrations;
    /// only the user needs code, because its password must be hashed.
    /// </summary>
    public static async Task SeedSuperAdmin(
        QrSanadSqlContext context,
        AdminSeedSettings settings,
        PasswordPolicySettings passwordPolicy,
        IPasswordService passwordService,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(a => a.UserType == UserType.SuperAdmin, cancellationToken))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Password))
        {
            logger.LogWarning(
                "No SuperAdmin exists and Persistence:Seed:Password is not set, so none was created. " +
                "Set it with user-secrets or the Persistence__Seed__Password environment variable and restart.");
            return;
        }

        if (!passwordPolicy.IsSatisfiedBy(settings.Password))
        {
            throw new InvalidOperationException("Persistence:Seed:Password does not satisfy the password policy.");
        }

        context.Users.Add(new User
        {
            Id = Generator.CreateV7Guid(),
            UserName = settings.UserName,
            NormalizedUserName = Normalizer.Normalize(settings.UserName),
            Email = settings.Email,
            NormalizedEmail = Normalizer.Normalize(settings.Email),
            DisplayName = settings.DisplayName,
            UserType = UserType.SuperAdmin,
            PasswordHash = passwordService.Hash(settings.Password),
            SecurityStamp = Generator.NewStamp(),
            ConcurrencyStamp = Generator.NewStamp(),
            Status = UserStatus.Active,
            MustChangePassword = true
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created SuperAdmin '{UserName}'. The password must be changed at first login.", settings.UserName);
    }
}
