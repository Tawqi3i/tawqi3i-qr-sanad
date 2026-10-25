namespace Tawqi3i.QrSanad.Identity;

using Microsoft.AspNetCore.Identity;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// ASP.NET Core Identity's hasher: PBKDF2-HMAC-SHA512, 100,000 iterations, per-password salt,
/// with the parameters stored in the hash so they can be raised later. Hashes made with weaker
/// parameters report SuccessRehashNeeded and are upgraded at the next login.
/// </summary>
internal sealed class PasswordService : IPasswordService
{
    private static readonly User HashSubject = new();

    private readonly PasswordHasher<User> hasher = new();
    private readonly Lazy<string> dummyHash;

    public PasswordService()
    {
        this.dummyHash = new Lazy<string>(() => this.hasher.HashPassword(HashSubject, Guid.NewGuid().ToString()));
    }

    public string Hash(string password) => this.hasher.HashPassword(HashSubject, password);

    public PasswordCheck Verify(string passwordHash, string password)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return PasswordCheck.Failed;
        }

        try
        {
            return this.hasher.VerifyHashedPassword(HashSubject, passwordHash, password) switch
            {
                PasswordVerificationResult.Success => PasswordCheck.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SuccessRehashNeeded,
                _ => PasswordCheck.Failed
            };
        }
        catch (FormatException)
        {
            // A corrupt stored hash must not turn into a 500 on the login endpoint.
            return PasswordCheck.Failed;
        }
    }

    public void VerifyAgainstDummy(string password)
    {
        this.hasher.VerifyHashedPassword(HashSubject, this.dummyHash.Value, password);
    }
}
