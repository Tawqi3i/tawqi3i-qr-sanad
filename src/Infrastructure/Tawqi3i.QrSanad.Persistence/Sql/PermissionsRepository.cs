namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class PermissionsRepository : RepositoryBase<Permission>, IPermissionsRepository
{
    public PermissionsRepository(QrSanadSqlContext context)
        : base(context)
    {
    }

    public async Task<ICollection<Permission>> GetByCodes(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        var distinct = codes.Distinct(StringComparer.Ordinal).ToList();

        if (distinct.Count == 0)
        {
            return [];
        }

        return await this.GetQueryable().Where(a => distinct.Contains(a.Code)).ToListAsync(cancellationToken);
    }
}
