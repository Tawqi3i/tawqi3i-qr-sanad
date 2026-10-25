namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

public interface IRefreshTokensRepository : IRepository<RefreshToken>, IUnitOnlyRepository<RefreshToken>
{
    Task<RefreshToken?> FindByHash(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the token only if it is still active, as one atomic UPDATE. Returns false when
    /// another request revoked it first, so two concurrent refreshes cannot both succeed.
    /// </summary>
    Task<bool> TryRevoke(Guid tokenId, DateTime now, string? ipAddress, string reason, Guid? replacedByTokenId = null, CancellationToken cancellationToken = default);

    Task<int> RevokeAllActive(Guid userId, DateTime now, string? ipAddress, string reason, CancellationToken cancellationToken = default);

    Task<int> RevokeAllActiveInBranch(Guid branchId, DateTime now, string? ipAddress, string reason, CancellationToken cancellationToken = default);
}
