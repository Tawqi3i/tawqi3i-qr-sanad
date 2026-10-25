namespace Tawqi3i.QrSanad.Common.Persistence;

/// <summary>
/// All repositories in a request share one DbContext, so SaveChanges persists everything staged
/// by any of them in a single database transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChanges(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="operation"/> in an explicit transaction, for work that mixes
    /// set-based Update/Delete calls with staged changes. Runs through the provider's execution
    /// strategy, so with retries enabled the operation may run more than once: load what it
    /// changes inside the operation rather than before it.
    /// </summary>
    Task ExecuteInTransaction(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
