namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

public interface ILicenseRepository : IRepository<License>, IUnitOnlyRepository<License>
{
    Task<string?> GetStoredKey(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages the key as the installed license; IUnitOfWork.SaveChanges persists it.
    /// </summary>
    Task StageKey(string key, CancellationToken cancellationToken = default);
}
