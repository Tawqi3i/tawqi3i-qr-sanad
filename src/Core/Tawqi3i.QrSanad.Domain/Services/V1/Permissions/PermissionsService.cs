namespace Tawqi3i.QrSanad.Domain.Services.V1.Permissions;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

internal sealed class PermissionsService : IPermissionsService
{
    private readonly IPermissionsRepository permissionsRepository;

    public PermissionsService(IPermissionsRepository permissionsRepository)
    {
        this.permissionsRepository = permissionsRepository;
    }

    public async Task<Response<ICollection<PermissionViewModel>>> GetAll(CancellationToken cancellationToken = default)
    {
        var permissions = await this.permissionsRepository.GetQueryable()
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

        return Response<ICollection<PermissionViewModel>>.Succeed(permissions.Select(ViewModelFactory.Create).ToList());
    }
}
