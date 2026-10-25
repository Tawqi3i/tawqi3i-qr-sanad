namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class BranchesRepository : RepositoryBase<Branch>, IBranchesRepository
{
    public BranchesRepository(QrSanadSqlContext context)
        : base(context)
    {
    }

    public override IQueryable<Branch> GetQueryable() => base.GetQueryable().Where(a => !a.IsDeleted);

    public override IQueryable<Branch> GetTrackedQueryable() => base.GetTrackedQueryable().Where(a => !a.IsDeleted);

    // Deleted branches are included on purpose: their names and codes stay reserved.
    public Task<bool> NameExists(string normalizedName, Guid? excludeBranchId = null, CancellationToken cancellationToken = default)
    {
        return this.context.Branches.AnyAsync(a => a.NormalizedName == normalizedName && a.Id != excludeBranchId, cancellationToken);
    }

    public Task<bool> CodeExists(string code, Guid? excludeBranchId = null, CancellationToken cancellationToken = default)
    {
        return this.context.Branches.AnyAsync(a => a.Code == code && a.Id != excludeBranchId, cancellationToken);
    }
}
