namespace Tawqi3i.QrSanad.Common.Constants;

/// <summary>
/// Response messages. The English text is also the localisation key, translated in
/// Tawqi3i.QrSanad.WebApi/Resources/SharedResource.ar.resx.
/// </summary>
public static class ResponseMessages
{
    public const string Success = "Success";
    public const string Created = "Created successfully";
    public const string Updated = "Updated successfully";
    public const string Deleted = "Deleted successfully";
    public const string SomethingWentWrong = "Sorry, something went wrong";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Access is forbidden";
    public const string TooManyRequests = "Too many requests. Try again later";
    public const string ValidationFailed = "One or more fields are invalid";
    public const string ConcurrencyConflict = "The record was changed by another request. Reload it and try again";
    public const string DuplicateRecord = "A record with the same value already exists";

    // Authentication
    public const string InvalidCredentials = "Invalid username or password";
    public const string AccountLocked = "Account is temporarily locked. Try again later";
    public const string AccountDisabled = "Account is disabled";
    public const string InvalidRefreshToken = "Invalid or expired refresh token";
    public const string LoggedOut = "Logged out successfully";
    public const string PasswordChanged = "Password changed successfully";
    public const string PasswordReset = "Password reset successfully";
    public const string CurrentPasswordIncorrect = "Current password is incorrect";
    public const string PasswordSameAsCurrent = "New password must be different from the current password";
    public const string PasswordTooWeak = "Password does not meet the password policy";
    public const string PasswordChangeRequired = "You must change your password before continuing";
    public const string PasswordPolicyLengthInvalid = "The maximum length must not be less than the minimum length";
    public const string PasswordPolicyUpdated = "Password policy updated. Every user must change their password";

    // Users
    public const string UserNotFound = "User not found";
    public const string UserNameTaken = "Username is already in use";
    public const string EmailTaken = "Email is already in use";
    public const string CannotModifySelf = "You cannot perform this action on your own account";
    public const string CannotManageUser = "You cannot manage this user";
    public const string CannotCreateUserType = "You cannot create this type of user";
    public const string UserUnlocked = "User unlocked successfully";

    // Branches
    public const string BranchNotFound = "Branch not found";
    public const string BranchNameTaken = "Branch name is already in use";
    public const string BranchCodeTaken = "Branch code is already in use";
    public const string BranchRequired = "A branch is required for this type of user";
    public const string BranchInactive = "The branch is disabled";
    public const string BranchHasAdmin = "The branch already has an admin";

    // Permissions
    public const string UnknownPermissions = "One or more permissions do not exist";
    public const string PrivilegeEscalation = "You cannot grant or revoke permissions you do not have";

    // QR Sanad
    public const string QRSanadUnavailable = "The e-seal service is unavailable. Try again later";
    public const string QrCodeInvalid = "The QR code is invalid";

    // Licensing
    public const string LicenseMissing = "No valid license is installed";
    public const string LicenseInvalid = "The license key is invalid";
    public const string LicenseExpired = "The license has expired. Contact your provider to renew it";
    public const string LicenseUserLimitReached = "The license user limit has been reached";
    public const string LicenseBranchLimitReached = "The license branch limit has been reached";
    public const string LicenseUpdated = "License updated successfully";
    public const string ScanStatsRangeInvalid = "The start date must be before the end date, at most 92 days apart";
}
