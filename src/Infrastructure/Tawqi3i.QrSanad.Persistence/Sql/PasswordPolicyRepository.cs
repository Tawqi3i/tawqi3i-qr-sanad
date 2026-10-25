namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.Security;

internal sealed class PasswordPolicyRepository : RepositoryBase<PasswordPolicy>, IPasswordPolicyRepository
{
    private readonly PasswordPolicySettings defaults;

    public PasswordPolicyRepository(QrSanadSqlContext context, PasswordPolicySettings defaults)
        : base(context)
    {
        this.defaults = defaults;
    }

    public async Task<PasswordPolicySettings> GetCurrent(CancellationToken cancellationToken = default)
    {
        var stored = await this.GetQueryable().FirstOrDefaultAsync(a => a.Id == PasswordPolicy.SingletonId, cancellationToken);

        return stored == null ? this.defaults : PasswordPolicySettings.From(stored);
    }
}
