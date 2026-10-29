namespace Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// The fixed hierarchy. Who may manage whom is decided by the type; what an admin may do beyond
/// that is decided by their permissions.
/// </summary>
public enum UserType
{
    /// <summary>
    /// The single system administrator: creates branches and their admins, sees everything,
    /// holds every permission. Seeded at startup; cannot be created through the API.
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Runs one branch: manages that branch's devices, within the permissions the SuperAdmin granted.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// A scanning station of one branch. The only type that can scan; holds no permissions.
    /// </summary>
    Device = 3,
}
