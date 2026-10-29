namespace Tawqi3i.QrSanad.Domain.Services.V1.Users;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

/// <summary>
/// Reached only by the SuperAdmin ([SuperAdminOnly]). Apart from the SuperAdmin itself, every
/// account here is a branch admin.
/// </summary>
internal sealed class UsersService : IUsersService
{
    private readonly IUsersRepository usersRepository;
    private readonly IPermissionsRepository permissionsRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserAccessProvider accessProvider;
    private readonly ISecurityAuditService auditService;
    private readonly ILicenseService licenseService;
    private readonly AccountOperations accounts;

    public UsersService(
        IUsersRepository usersRepository,
        IPermissionsRepository permissionsRepository,
        IUnitOfWork unitOfWork,
        IUserAccessProvider accessProvider,
        ISecurityAuditService auditService,
        ILicenseService licenseService,
        AccountOperations accounts)
    {
        this.usersRepository = usersRepository;
        this.permissionsRepository = permissionsRepository;
        this.unitOfWork = unitOfWork;
        this.accessProvider = accessProvider;
        this.auditService = auditService;
        this.licenseService = licenseService;
        this.accounts = accounts;
    }

    public Task<PagedResponse<UserViewModel>> GetAll(UsersQuery query, CancellationToken cancellationToken = default)
    {
        return this.accounts.GetPage(AccessGuard.Users(this.usersRepository.GetQueryable()), query, cancellationToken);
    }

    public async Task<Response<UserDetailsViewModel>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await AccessGuard.Users(this.usersRepository.GetQueryable()).AnyAsync(a => a.Id == id, cancellationToken);

        return exists
            ? await this.accounts.GetDetails(id, cancellationToken)
            : Response<UserDetailsViewModel>.NotFound(ResponseMessages.UserNotFound);
    }

    public async Task<Response<UserDetailsViewModel>> Create(UserCreateRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (branch, branchDenied) = await this.accounts.GetActiveBranch(request.BranchId!.Value, cancellationToken);

        if (branchDenied != null)
        {
            return Response<UserDetailsViewModel>.From(branchDenied);
        }

        if (branch!.AdminUserId != null)
        {
            return Response<UserDetailsViewModel>.Conflict(ResponseMessages.BranchHasAdmin);
        }

        if (await this.licenseService.EnsureCanAddUser(cancellationToken) is { } licenseDenied)
        {
            return Response<UserDetailsViewModel>.From(licenseDenied);
        }

        if (await this.accounts.ValidateNewAccount(request.UserName, request.Email, request.Password, cancellationToken) is { } invalid)
        {
            return Response<UserDetailsViewModel>.From(invalid);
        }

        // A new admin gets every permission unless the SuperAdmin chose a narrower set.
        var codes = NormaliseCodes(request.Permissions);

        if (codes.Count == 0)
        {
            codes = PermissionCodes.AllCodes.ToList();
        }

        var permissions = await this.permissionsRepository.GetByCodes(codes, cancellationToken);

        if (permissions.Count != codes.Count)
        {
            return Response<UserDetailsViewModel>.Fail(ResponseMessages.UnknownPermissions);
        }

        var admin = DomainModelFactory.CreateAdmin(request, branch.Id, this.accounts.HashPassword(request.Password), permissions, actor.Id, this.accounts.Now);

        branch.AdminUserId = admin.Id;
        branch.ConcurrencyStamp = Generator.NewStamp();

        return await this.accounts.Add(admin, branch, actor, cancellationToken);
    }

    public async Task<Response<UserDetailsViewModel>> Update(Guid id, UserUpdateRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.Find(id, actor, allowSelf: true, cancellationToken);

        if (denied != null)
        {
            return Response<UserDetailsViewModel>.From(denied);
        }

        var email = request.Email.Trim();
        var normalizedEmail = Normalizer.Normalize(email);

        if (normalizedEmail != user!.NormalizedEmail && await this.usersRepository.EmailExists(normalizedEmail, id, cancellationToken))
        {
            return Response<UserDetailsViewModel>.Conflict(ResponseMessages.EmailTaken);
        }

        user.Email = email;
        user.NormalizedEmail = normalizedEmail;
        user.DisplayName = request.DisplayName.Trim();
        user.PhoneNumber = DomainModelFactory.NullIfBlank(request.PhoneNumber);

        return await this.accounts.SaveUpdate(user, actor, cancellationToken);
    }

    public async Task<Response> SetStatus(Guid id, UserStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.Find(id, actor, allowSelf: false, cancellationToken);

        return denied ?? await this.accounts.SetStatus(user!, request, actor, cancellationToken);
    }

    public async Task<Response> Unlock(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.Find(id, actor, allowSelf: false, cancellationToken);

        return denied ?? await this.accounts.Unlock(user!, actor, cancellationToken);
    }

    public async Task<Response> ResetPassword(Guid id, ResetPasswordRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        // Your own password goes through auth/password, which checks the current one.
        var (user, denied) = await this.Find(id, actor, allowSelf: false, cancellationToken);

        return denied ?? await this.accounts.ResetPassword(user!, request, actor, cancellationToken);
    }

    public async Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.Find(id, actor, allowSelf: false, cancellationToken);

        return denied ?? await this.accounts.Delete(user!, actor, cancellationToken);
    }

    public async Task<Response<UserDetailsViewModel>> SetPermissions(Guid id, PermissionsRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.Find(id, actor, allowSelf: false, cancellationToken, includePermissions: true);

        if (denied != null)
        {
            return Response<UserDetailsViewModel>.From(denied);
        }

        var requestedCodes = NormaliseCodes(request.Permissions);
        var permissions = await this.permissionsRepository.GetByCodes(requestedCodes, cancellationToken);

        if (permissions.Count != requestedCodes.Count)
        {
            return Response<UserDetailsViewModel>.Fail(ResponseMessages.UnknownPermissions);
        }

        var currentCodes = user!.UserPermissions.Select(a => a.Permission.Code).ToHashSet(StringComparer.Ordinal);
        var added = permissions.Where(a => !currentCodes.Contains(a.Code)).ToList();
        var removed = user.UserPermissions.Where(a => !requestedCodes.Contains(a.Permission.Code, StringComparer.Ordinal)).ToList();

        if (added.Count == 0 && removed.Count == 0)
        {
            return await this.accounts.GetDetails(id, cancellationToken);
        }

        var now = this.accounts.Now;

        foreach (var userPermission in removed)
        {
            user.UserPermissions.Remove(userPermission);
        }

        foreach (var permission in added)
        {
            user.UserPermissions.Add(new UserPermission { UserId = user.Id, PermissionId = permission.Id, AssignedAt = now, AssignedBy = actor.Id });
        }

        user.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(
            SecurityEventType.UserPermissionsChanged,
            actor,
            user.Id,
            user.UserName,
            actor.Id,
            user.BranchId,
            $"added: [{string.Join(", ", added.Select(a => a.Code))}]; removed: [{string.Join(", ", removed.Select(a => a.Permission.Code))}]");

        await this.unitOfWork.SaveChanges(cancellationToken);
        await this.accessProvider.Invalidate(user.Id, cancellationToken);

        return await this.accounts.GetDetails(id, cancellationToken);
    }

    private static List<string> NormaliseCodes(IEnumerable<string> codes)
    {
        return codes.Select(a => a.Trim()).Where(a => a.Length > 0).Distinct(StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// The only account here besides the admins is the SuperAdmin, who is the actor, so the
    /// self check is what keeps it out of reach.
    /// </summary>
    private Task<(User? User, Response? Denied)> Find(
        Guid id,
        RequestUser actor,
        bool allowSelf,
        CancellationToken cancellationToken,
        bool includePermissions = false)
    {
        var users = this.usersRepository.GetTrackedQueryable();

        if (includePermissions)
        {
            users = users.Include(a => a.UserPermissions).ThenInclude(a => a.Permission);
        }

        return this.accounts.Find(AccessGuard.Users(users), id, actor, allowSelf, ResponseMessages.UserNotFound, cancellationToken);
    }
}
