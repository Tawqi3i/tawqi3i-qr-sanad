namespace Tawqi3i.QrSanad.Persistence.Sql;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Tawqi3i.QrSanad.Common.Persistence;

internal abstract class RepositoryBase<T> : IRepository<T>, IUnitOnlyRepository<T>
    where T : class
{
    protected readonly QrSanadSqlContext context;

    protected RepositoryBase(QrSanadSqlContext context)
    {
        this.context = context;
    }

    public virtual IQueryable<T> GetQueryable() => this.context.Set<T>().AsNoTracking();

    public virtual IQueryable<T> GetTrackedQueryable() => this.context.Set<T>();

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> filter) => this.GetQueryable().Where(filter);

    public async Task Add(T model, CancellationToken cancellationToken = default)
    {
        this.context.Set<T>().Add(model);
        await this.SaveChanges(cancellationToken);
    }

    public async Task AddRange(ICollection<T> models, CancellationToken cancellationToken = default)
    {
        this.context.Set<T>().AddRange(models);
        await this.SaveChanges(cancellationToken);
    }

    public Task<int> Update(Expression<Func<T, bool>> filter, Action<UpdateSettersBuilder<T>> setters, CancellationToken cancellationToken = default)
    {
        return this.GetTrackedQueryable().Where(filter).ExecuteUpdateAsync(setters, cancellationToken);
    }

    public Task<int> Delete(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return this.GetTrackedQueryable().Where(filter).ExecuteDeleteAsync(cancellationToken);
    }

    public void AddOnly(T model) => this.context.Set<T>().Add(model);

    public void AddOnlyRange(IEnumerable<T> models) => this.context.Set<T>().AddRange(models);

    public void DeleteOnly(T model) => this.context.Set<T>().Remove(model);

    public void DeleteOnlyRange(IEnumerable<T> models) => this.context.Set<T>().RemoveRange(models);

    private async Task SaveChanges(CancellationToken cancellationToken)
    {
        try
        {
            await this.context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e) when (PersistenceExceptionTranslator.TryTranslate(e, out var translated))
        {
            throw translated;
        }
    }
}
