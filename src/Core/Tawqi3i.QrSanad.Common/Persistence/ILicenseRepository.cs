namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

public interface ILicenseRepository : IRepository<License>, IUnitOnlyRepository<License>
{
    Task<string?> GetStoredKey(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages the key as the installed license; IUnitOfWork.SaveChanges persists it.
    /// </summary>
    Task StageKey(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new concurrency stamp on the license row. A limit is checked by counting and then
    /// adding, so two requests at once can both see room for the last place; saving the new stamp
    /// with the added row makes the second save fail with a concurrency conflict instead. Call it
    /// before counting.
    /// </summary>
    Task StageLimitClaim(CancellationToken cancellationToken = default);
}
