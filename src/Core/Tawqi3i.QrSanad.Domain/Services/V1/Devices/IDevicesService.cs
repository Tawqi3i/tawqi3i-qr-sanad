namespace Tawqi3i.QrSanad.Domain.Services.V1.Devices;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// Every operation is scoped to what the actor may see: the SuperAdmin sees the devices of all
/// branches, an admin only their own branch's. Devices outside that scope are reported as not found.
/// </summary>
public interface IDevicesService
{
    Task<PagedResponse<UserViewModel>> GetAll(UsersQuery query, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> Get(Guid id, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> Create(DeviceCreateRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> Update(Guid id, DeviceUpdateRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> SetStatus(Guid id, UserStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> Unlock(Guid id, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> ResetPassword(Guid id, ResetPasswordRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default);
}
