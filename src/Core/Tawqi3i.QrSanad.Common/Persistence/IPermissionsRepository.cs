namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

public interface IPermissionsRepository : IRepository<Permission>
{
    Task<ICollection<Permission>> GetByCodes(IEnumerable<string> codes, CancellationToken cancellationToken = default);
}
