namespace Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// The fixed hierarchy. Who may manage whom is decided by the type; what a user may do beyond
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
    /// Runs one branch: creates and manages that branch's users.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// A member of one branch, with the permissions their admin granted.
    /// </summary>
    User = 3,
}
