namespace Tawqi3i.QrSanad.Domain.Services.V1.Devices;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

internal sealed class DevicesService : IDevicesService
{
    private readonly IUsersRepository usersRepository;
    private readonly ILicenseService licenseService;
    private readonly AccessGuard accessGuard;
    private readonly AccountOperations accounts;

    public DevicesService(
        IUsersRepository usersRepository,
        ILicenseService licenseService,
        AccessGuard accessGuard,
        AccountOperations accounts)
    {
        this.usersRepository = usersRepository;
        this.licenseService = licenseService;
        this.accessGuard = accessGuard;
        this.accounts = accounts;
    }

    public async Task<PagedResponse<UserViewModel>> GetAll(UsersQuery query, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);

        return await this.accounts.GetPage(AccessGuard.DevicesVisibleTo(this.usersRepository.GetQueryable(), actorAccess), query, cancellationToken);
    }

    public async Task<Response<UserDetailsViewModel>> Get(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var visible = await AccessGuard.DevicesVisibleTo(this.usersRepository.GetQueryable(), actorAccess).AnyAsync(a => a.Id == id, cancellationToken);

        return visible
            ? await this.accounts.GetDetails(id, cancellationToken)
            : Response<UserDetailsViewModel>.NotFound(ResponseMessages.DeviceNotFound);
    }

    public async Task<Response<UserDetailsViewModel>> Create(DeviceCreateRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);

        // The SuperAdmin names the branch; an admin always adds to their own, and naming another
        // branch reads as not found, like everything else outside their scope.
        if (!actorAccess.IsSuperAdmin && request.BranchId != null && request.BranchId != actorAccess.BranchId)
        {
            return Response<UserDetailsViewModel>.NotFound(ResponseMessages.BranchNotFound);
        }

        if ((actorAccess.IsSuperAdmin ? request.BranchId : actorAccess.BranchId) is not { } branchId)
        {
            return Response<UserDetailsViewModel>.Fail(ResponseMessages.BranchRequired);
        }

        var (branch, branchDenied) = await this.accounts.GetActiveBranch(branchId, cancellationToken);

        if (branchDenied != null)
        {
            return Response<UserDetailsViewModel>.From(branchDenied);
        }

        if (await this.licenseService.EnsureCanAddDevice(cancellationToken) is { } licenseDenied)
        {
            return Response<UserDetailsViewModel>.From(licenseDenied);
        }

        if (await this.accounts.ValidateNewAccount(request.UserName, null, request.Password, cancellationToken) is { } invalid)
        {
            return Response<UserDetailsViewModel>.From(invalid);
        }

        var device = DomainModelFactory.CreateDevice(request, branch!.Id, this.accounts.HashPassword(request.Password), actor.Id, this.accounts.Now);

        return await this.accounts.Add(device, branch, actor, cancellationToken);
    }

    public async Task<Response<UserDetailsViewModel>> Update(Guid id, DeviceUpdateRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (device, denied) = await this.Find(id, actor, cancellationToken);

        if (denied != null)
        {
            return Response<UserDetailsViewModel>.From(denied);
        }

        device!.DisplayName = request.DisplayName.Trim();

        return await this.accounts.SaveUpdate(device, actor, cancellationToken);
    }

    public async Task<Response> SetStatus(Guid id, UserStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (device, denied) = await this.Find(id, actor, cancellationToken);

        return denied ?? await this.accounts.SetStatus(device!, request, actor, cancellationToken);
    }

    public async Task<Response> Unlock(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (device, denied) = await this.Find(id, actor, cancellationToken);

        return denied ?? await this.accounts.Unlock(device!, actor, cancellationToken);
    }

    public async Task<Response> ResetPassword(Guid id, ResetPasswordRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (device, denied) = await this.Find(id, actor, cancellationToken);

        return denied ?? await this.accounts.ResetPassword(device!, request, actor, cancellationToken);
    }

    public async Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (device, denied) = await this.Find(id, actor, cancellationToken);

        return denied ?? await this.accounts.Delete(device!, actor, cancellationToken);
    }

    private async Task<(User? Device, Response? Denied)> Find(Guid id, RequestUser actor, CancellationToken cancellationToken)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var devices = AccessGuard.DevicesVisibleTo(this.usersRepository.GetTrackedQueryable(), actorAccess);

        return await this.accounts.Find(devices, id, actor, allowSelf: false, ResponseMessages.DeviceNotFound, cancellationToken);
    }
}
