namespace Tawqi3i.QrSanad.Common.Identity;

public interface IPasswordService
{
    string Hash(string password);

    PasswordCheck Verify(string passwordHash, string password);

    /// <summary>
    /// Burns the same time as a real verification, so a login for an unknown user is not
    /// measurably faster than one for a known user.
    /// </summary>
    void VerifyAgainstDummy(string password);
}

public enum PasswordCheck
{
    Failed,
    Success,
    SuccessRehashNeeded,
}
