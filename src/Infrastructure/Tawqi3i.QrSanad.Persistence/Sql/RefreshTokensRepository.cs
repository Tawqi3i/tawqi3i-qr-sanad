namespace Tawqi3i.QrSanad.Persistence.Sql;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class RefreshTokensRepository : RepositoryBase<RefreshToken>, IRefreshTokensRepository
{
    public RefreshTokensRepository(QrSanadSqlContext context)
        : base(context)
    {
    }

    public Task<RefreshToken?> FindByHash(string tokenHash, CancellationToken cancellationToken = default)
    {
        return this.GetQueryable().FirstOrDefaultAsync(a => a.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<bool> TryRevoke(Guid tokenId, DateTime now, string? ipAddress, string reason, Guid? replacedByTokenId = null, CancellationToken cancellationToken = default)
    {
        var updated = await this.Update(
            a => a.Id == tokenId && a.RevokedAt == null,
            setters => setters
                .SetProperty(a => a.RevokedAt, now)
                .SetProperty(a => a.RevokedByIp, ipAddress)
                .SetProperty(a => a.RevokeReason, reason)
                .SetProperty(a => a.ReplacedByTokenId, replacedByTokenId),
            cancellationToken);

        return updated == 1;
    }

    public Task<int> RevokeAllActive(Guid userId, DateTime now, string? ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        return this.Update(
            a => a.UserId == userId && a.RevokedAt == null,
            setters => setters
                .SetProperty(a => a.RevokedAt, now)
                .SetProperty(a => a.RevokedByIp, ipAddress)
                .SetProperty(a => a.RevokeReason, reason),
            cancellationToken);
    }

    public Task<int> RevokeAllActiveInBranch(Guid branchId, DateTime now, string? ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        return this.Update(
            a => a.User.BranchId == branchId && a.RevokedAt == null,
            setters => setters
                .SetProperty(a => a.RevokedAt, now)
                .SetProperty(a => a.RevokedByIp, ipAddress)
                .SetProperty(a => a.RevokeReason, reason),
            cancellationToken);
    }
}
