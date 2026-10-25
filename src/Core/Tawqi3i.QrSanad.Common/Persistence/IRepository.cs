namespace Tawqi3i.QrSanad.Common.Persistence;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// To change an entity, read it through <see cref="GetTrackedQueryable"/>, modify it and call
/// IUnitOfWork.SaveChanges: EF then writes only the columns that changed. There is deliberately
/// no full-entity Update(model) — writing back a detached entity overwrites every column,
/// including ones another request changed in the meantime.
/// </summary>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// No-tracking query, for reads.
    /// </summary>
    IQueryable<T> GetQueryable();

    /// <summary>
    /// Tracked query, for read-modify-write.
    /// </summary>
    IQueryable<T> GetTrackedQueryable();

    IQueryable<T> FindByCondition(Expression<Func<T, bool>> filter);

    /// <summary>
    /// Adds and saves immediately.
    /// </summary>
    Task Add(T model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds and saves immediately.
    /// </summary>
    Task AddRange(ICollection<T> models, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set-based UPDATE executed straight away, bypassing the change tracker.
    /// </summary>
    Task<int> Update(Expression<Func<T, bool>> filter, Action<UpdateSettersBuilder<T>> setters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set-based DELETE executed straight away, bypassing the change tracker.
    /// </summary>
    Task<int> Delete(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
}
