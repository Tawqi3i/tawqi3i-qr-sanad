namespace Tawqi3i.QrSanad.Common.Persistence;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Security;

public interface IPasswordPolicyRepository : IRepository<PasswordPolicy>, IUnitOnlyRepository<PasswordPolicy>
{
    /// <summary>
    /// The policy in force: the stored row, or the Identity:PasswordPolicy defaults when none exists yet.
    /// </summary>
    Task<PasswordPolicySettings> GetCurrent(CancellationToken cancellationToken = default);
}
