namespace Tawqi3i.QrSanad.Common.Enums;

/// <summary>
/// Stored as its name, so values can be reordered or inserted freely.
/// </summary>
public enum SecurityEventType
{
    LoginSucceeded,
    LoginFailed,
    AccountLocked,
    Logout,
    LogoutAll,
    RefreshTokenReuseDetected,
    PasswordChanged,
    PasswordReset,
    UserCreated,
    UserUpdated,
    UserStatusChanged,
    UserUnlocked,
    UserDeleted,
    UserPermissionsChanged,
    BranchCreated,
    BranchUpdated,
    BranchStatusChanged,
    BranchDeleted,
    QrScanSucceeded,
    QrScanFailed,
    PasswordPolicyUpdated,
    LicenseUpdated,
}
