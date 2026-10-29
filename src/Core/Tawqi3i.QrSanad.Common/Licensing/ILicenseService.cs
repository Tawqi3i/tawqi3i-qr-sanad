namespace Tawqi3i.QrSanad.Common.Licensing;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Licensing;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// The installed license and the limits it sets. The Ensure methods return the failure to send,
/// or null when the operation is allowed. The EnsureCanAdd methods also stage a claim on the
/// license, so call them in the unit of work that adds the row: if another request takes the
/// last place in between, that save fails with a concurrency conflict.
/// </summary>
public interface ILicenseService
{
    Task<Response<LicenseViewModel>> Get(CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs a new license key. It must be valid and not expired.
    /// </summary>
    Task<Response<LicenseViewModel>> Update(LicenseRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fails when no valid license is installed or it has expired.
    /// </summary>
    Task<Response?> EnsureActive(CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="EnsureActive"/>, plus room for one more admin.
    /// </summary>
    Task<Response?> EnsureCanAddUser(CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="EnsureActive"/>, plus room for one more device.
    /// </summary>
    Task<Response?> EnsureCanAddDevice(CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="EnsureActive"/>, plus room for one more branch.
    /// </summary>
    Task<Response?> EnsureCanAddBranch(CancellationToken cancellationToken = default);
}
