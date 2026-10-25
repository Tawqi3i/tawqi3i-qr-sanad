namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly QrSanadSqlContext context;

    public UnitOfWork(QrSanadSqlContext context)
    {
        this.context = context;
    }

    public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e) when (PersistenceExceptionTranslator.TryTranslate(e, out var translated))
        {
            throw translated;
        }
    }

    public Task ExecuteInTransaction(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        // A user-initiated transaction must go through the execution strategy, otherwise a
        // retrying strategy (EnableRetryOnFailure) refuses to run it.
        var strategy = this.context.Database.CreateExecutionStrategy();
        var attempt = 0;

        return strategy.ExecuteAsync(
            async token =>
            {
                // On a retry, drop what the failed attempt staged so it is not saved twice.
                if (attempt++ > 0)
                {
                    this.context.ChangeTracker.Clear();
                }

                await using var transaction = await this.context.Database.BeginTransactionAsync(token);
                await operation(token);
                await transaction.CommitAsync(token);
            },
            cancellationToken);
    }
}
