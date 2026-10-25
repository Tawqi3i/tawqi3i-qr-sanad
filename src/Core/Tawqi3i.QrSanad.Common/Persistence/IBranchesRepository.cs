namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// Every query excludes soft-deleted branches.
/// </summary>
public interface IBranchesRepository : IRepository<Branch>, IUnitOnlyRepository<Branch>
{
    Task<bool> NameExists(string normalizedName, Guid? excludeBranchId = null, CancellationToken cancellationToken = default);

    Task<bool> CodeExists(string code, Guid? excludeBranchId = null, CancellationToken cancellationToken = default);
}
