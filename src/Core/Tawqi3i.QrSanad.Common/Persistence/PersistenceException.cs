namespace Tawqi3i.QrSanad.Common.Persistence;

/// <summary>
/// A database error translated into a provider-neutral code from <see cref="ErrorCodes"/>.
/// </summary>
public class PersistenceException : Exception
{
    public PersistenceException(string message, string code, Exception? innerException = null)
        : base(message, innerException)
    {
        this.Code = code;
    }

    public string Code { get; }
}

public static class ErrorCodes
{
    public const string DuplicateKey = "duplicate_key";

    public const string InvalidForeignKey = "invalid_foreign_key";

    public const string ConcurrencyConflict = "concurrency_conflict";
}
