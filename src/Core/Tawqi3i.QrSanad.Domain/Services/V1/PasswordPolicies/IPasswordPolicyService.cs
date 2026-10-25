namespace Tawqi3i.QrSanad.Domain.Services.V1.PasswordPolicies;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

public interface IPasswordPolicyService
{
    Task<Response<PasswordPolicyViewModel>> Get(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the policy and requires every user, the SuperAdmin included, to choose a new
    /// password that satisfies it. Sessions stay open, but only change-password, profile and
    /// logout work until the password is changed.
    /// </summary>
    Task<Response<PasswordPolicyViewModel>> Update(PasswordPolicyRequest request, RequestUser actor, CancellationToken cancellationToken = default);
}
