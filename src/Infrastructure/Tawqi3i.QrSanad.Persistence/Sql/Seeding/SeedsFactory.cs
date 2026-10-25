namespace Tawqi3i.QrSanad.Persistence.Sql.Seeding;

using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Helpers;

/// <summary>
/// Reference data seeded through HasData, so it is part of every provider's migrations and of
/// the generated SQL scripts. Every value must be constant: a changed value (a new Guid,
/// DateTime.UtcNow) would make each new migration rewrite the rows.
/// </summary>
internal static class SeedsFactory
{
    public static Guid PermissionId(string code) => Generator.CreateDeterministicGuid($"qrsanad:permission:{code}");

    public static IEnumerable<Permission> GetPermissions()
    {
        return PermissionCodes.All.Select(a => new Permission
        {
            Id = PermissionId(a.Code),
            Code = a.Code,
            Description = a.Description
        });
    }
}
