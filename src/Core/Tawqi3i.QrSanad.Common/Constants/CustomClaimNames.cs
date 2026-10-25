namespace Tawqi3i.QrSanad.Common.Constants;

public static class CustomClaimNames
{
    /// <summary>
    /// The user's security stamp when the token was issued. Rotating the stamp (password change,
    /// disable, delete, "log out everywhere") invalidates every access token issued before it.
    /// </summary>
    public const string SecurityStamp = "ss";

    /// <summary>
    /// Present while the user must change their password. Permission-protected endpoints reject
    /// such tokens; only profile, change-password and logout stay reachable.
    /// </summary>
    public const string PasswordChangeRequired = "pcr";
}
