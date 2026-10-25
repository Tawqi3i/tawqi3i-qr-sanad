namespace Tawqi3i.QrSanad.Domain.Services.V1.Permissions;

using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

public interface IPermissionsService
{
    Task<Response<ICollection<PermissionViewModel>>> GetAll(CancellationToken cancellationToken = default);
}
