namespace Tawqi3i.QrSanad.Domain.Services.V1.Users;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// The SuperAdmin's management of the branch admins. Devices are managed by IDevicesService and
/// are reported as not found here.
/// </summary>
public interface IUsersService
{
    Task<PagedResponse<UserViewModel>> GetAll(UsersQuery query, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> Get(Guid id, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> Create(UserCreateRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> Update(Guid id, UserUpdateRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> SetStatus(Guid id, UserStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> Unlock(Guid id, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> ResetPassword(Guid id, ResetPasswordRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<UserDetailsViewModel>> SetPermissions(Guid id, PermissionsRequest request, RequestUser actor, CancellationToken cancellationToken = default);
}
