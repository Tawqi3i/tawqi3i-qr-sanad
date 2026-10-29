namespace Tawqi3i.QrSanad.Identity.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Requires an authenticated user holding <paramref name="permission"/> (see PermissionCodes).
/// The SuperAdmin holds every permission. Stacking several attributes requires all of them.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirementData
{
    public string Permission { get; } = permission;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new PermissionRequirement(this.Permission);
    }
}

/// <summary>
/// Requires the SuperAdmin. Used for what no permission can delegate: branches and their admins.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class SuperAdminOnlyAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new SuperAdminRequirement();
    }
}

/// <summary>
/// Requires a device. Used for scanning, which no admin may do whatever their permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class DeviceOnlyAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new DeviceRequirement();
    }
}

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class SuperAdminRequirement : IAuthorizationRequirement
{
}

public sealed class DeviceRequirement : IAuthorizationRequirement
{
}
