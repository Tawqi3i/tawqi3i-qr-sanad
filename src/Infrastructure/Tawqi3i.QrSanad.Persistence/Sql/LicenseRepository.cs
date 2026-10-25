namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class LicenseRepository : RepositoryBase<License>, ILicenseRepository
{
    public LicenseRepository(QrSanadSqlContext context)
        : base(context)
    {
    }

    public Task<string?> GetStoredKey(CancellationToken cancellationToken = default)
    {
        return this.GetQueryable()
            .Where(a => a.Id == License.SingletonId)
            .Select(a => a.Key)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task StageKey(string key, CancellationToken cancellationToken = default)
    {
        var entity = await this.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == License.SingletonId, cancellationToken);

        if (entity == null)
        {
            this.AddOnly(new License { Id = License.SingletonId, Key = key });
        }
        else
        {
            entity.Key = key;
        }
    }
}
