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

internal sealed class UsersService : IUsersService
{
    private const string AdminActionReason = "admin_action";

    private readonly IUsersRepository usersRepository;
    private readonly IBranchesRepository branchesRepository;
    private readonly IPermissionsRepository permissionsRepository;
    private readonly IRefreshTokensRepository refreshTokensRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPasswordService passwordService;
    private readonly IUserAccessProvider accessProvider;
    private readonly ISecurityAuditService auditService;
    private readonly AccessGuard accessGuard;
    private readonly IPasswordPolicyRepository passwordPolicyRepository;
    private readonly ILicenseService licenseService;
    private readonly TimeProvider timeProvider;

    public UsersService(
        IUsersRepository usersRepository,
        IBranchesRepository branchesRepository,
        IPermissionsRepository permissionsRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IUserAccessProvider accessProvider,
        ISecurityAuditService auditService,
        AccessGuard accessGuard,
        IPasswordPolicyRepository passwordPolicyRepository,
        ILicenseService licenseService,
        TimeProvider timeProvider)
    {
        this.usersRepository = usersRepository;
        this.branchesRepository = branchesRepository;
        this.permissionsRepository = permissionsRepository;
        this.refreshTokensRepository = refreshTokensRepository;
        this.unitOfWork = unitOfWork;
        this.passwordService = passwordService;
        this.accessProvider = accessProvider;
        this.auditService = auditService;
        this.accessGuard = accessGuard;
        this.passwordPolicyRepository = passwordPolicyRepository;
        this.licenseService = licenseService;
        this.timeProvider = timeProvider;
    }

    private DateTime Now => this.timeProvider.GetUtcNow().UtcDateTime;

    public async Task<PagedResponse<UserViewModel>> GetAll(UsersQuery query, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var users = AccessGuard.VisibleTo(this.usersRepository.GetQueryable(), actorAccess);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = Normalizer.Normalize(query.Search);
            users = users.Where(a => a.NormalizedUserName.Contains(term) || a.NormalizedEmail.Contains(term) || a.DisplayName.ToUpper().Contains(term));
        }

        if (query.Status is { } status)
        {
            users = users.Where(a => a.Status == status);
        }

        if (query.Type is { } type)
        {
            users = users.Where(a => a.UserType == type);
        }

        if (query.BranchId is { } branchId)
        {
            users = users.Where(a => a.BranchId == branchId);
        }

        var totalCount = await users.CountAsync(cancellationToken);

        var items = await users
            .Include(a => a.Branch)
            .OrderByDescending(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return ViewModelFactory.CreatePaginated(items, totalCount, query, this.Now);
    }

    public async Task<Response<UserDetailsViewModel>> Get(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var visible = await AccessGuard.VisibleTo(this.usersRepository.GetQueryable(), actorAccess).AnyAsync(a => a.Id == id, cancellationToken);

        return visible
            ? await this.GetDetails(id, cancellationToken)
            : Response<UserDetailsViewModel>.NotFound(ResponseMessages.UserNotFound);
    }

    public async Task<Response<UserDetailsViewModel>> Create(UserCreateRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);

        // The SuperAdmin creates admins and users for any branch; anyone else creates plain
        // users in their own branch only.
        Guid branchId;

        if (actorAccess.IsSuperAdmin)
        {
            if (request.Type == UserType.SuperAdmin)
            {
                return Response<UserDetailsViewModel>.Forbidden(ResponseMessages.CannotCreateUserType);
            }

            if (request.BranchId is not { } requestedBranchId)
            {
                return Response<UserDetailsViewModel>.Fail(ResponseMessages.BranchRequired);
            }

            branchId = requestedBranchId;
        }
        else
        {
            if (request.Type != UserType.User || actorAccess.BranchId is null || (request.BranchId != null && request.BranchId != actorAccess.BranchId))
            {
                return Response<UserDetailsViewModel>.Forbidden(ResponseMessages.CannotCreateUserType);
            }

            branchId = actorAccess.BranchId.Value;
        }

        var branch = await this.branchesRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == branchId, cancellationToken);

        if (branch == null)
        {
            return Response<UserDetailsViewModel>.NotFound(ResponseMessages.BranchNotFound);
        }

        if (branch.Status != BranchStatus.Active)
        {
            return Response<UserDetailsViewModel>.Fail(ResponseMessages.BranchInactive);
        }

        if (request.Type == UserType.Admin && branch.AdminUserId != null)
        {
            return Response<UserDetailsViewModel>.Conflict(ResponseMessages.BranchHasAdmin);
        }

        if (await this.licenseService.EnsureCanAddUser(cancellationToken) is { } licenseDenied)
        {
            return Response<UserDetailsViewModel>.From(licenseDenied);
        }

        var passwordPolicy = await this.passwordPolicyRepository.GetCurrent(cancellationToken);

        if (!passwordPolicy.IsSatisfiedBy(request.Password))
        {
            return Response<UserDetailsViewModel>.Fail(ResponseMessages.PasswordTooWeak);
        }

        if (await this.usersRepository.UserNameExists(Normalizer.Normalize(request.UserName), cancellationToken: cancellationToken))
        {
            return Response<UserDetailsViewModel>.Conflict(ResponseMessages.UserNameTaken);
        }

        if (await this.usersRepository.EmailExists(Normalizer.Normalize(request.Email), cancellationToken: cancellationToken))
        {
            return Response<UserDetailsViewModel>.Conflict(ResponseMessages.EmailTaken);
        }

        var codes = NormaliseCodes(request.Permissions);

        // A new admin gets every permission unless the SuperAdmin chose a narrower set.
        if (request.Type == UserType.Admin && codes.Count == 0)
        {
            codes = PermissionCodes.AllCodes.ToList();
        }

        var permissions = await this.permissionsRepository.GetByCodes(codes, cancellationToken);

        if (permissions.Count != codes.Count)
        {
            return Response<UserDetailsViewModel>.Fail(ResponseMessages.UnknownPermissions);
        }

        if (!AccessGuard.CanGrant(actorAccess, codes))
        {
            return Response<UserDetailsViewModel>.Forbidden(ResponseMessages.PrivilegeEscalation);
        }

        var user = DomainModelFactory.CreateUser(request, this.passwordService.Hash(request.Password), branch.Id, permissions, actor.Id, this.Now);

        this.usersRepository.AddOnly(user);

        if (user.UserType == UserType.Admin)
        {
            branch.AdminUserId = user.Id;
            branch.ConcurrencyStamp = Generator.NewStamp();
        }

        this.auditService.Record(SecurityEventType.UserCreated, actor, user.Id, user.UserName, actor.Id, branch.Id, $"{user.UserType} of branch {branch.Name}");
        await this.unitOfWork.SaveChanges(cancellationToken);

        var created = await this.GetDetails(user.Id, cancellationToken);
        return Response<UserDetailsViewModel>.Created(created.Data!);
    }

    public async Task<Response<UserDetailsViewModel>> Update(Guid id, UserUpdateRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.FindManageable(id, actor, allowSelf: true, cancellationToken);

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
        user.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.UserUpdated, actor, user.Id, user.UserName, actor.Id, user.BranchId);
        await this.unitOfWork.SaveChanges(cancellationToken);

        return await this.GetDetails(id, cancellationToken);
    }

    public async Task<Response> SetStatus(Guid id, UserStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.FindManageable(id, actor, allowSelf: false, cancellationToken);

        if (denied != null)
        {
            return denied;
        }

        if (user!.Status == request.Status)
        {
            return Response.Succeed(ResponseMessages.Updated);
        }

        var previous = user.Status;
        user.Status = request.Status;
        user.ConcurrencyStamp = Generator.NewStamp();

        if (request.Status == UserStatus.Disabled)
        {
            // Ends every session: access and refresh tokens carry the old stamp.
            user.SecurityStamp = Generator.NewStamp();
        }

        await this.SaveAndEndSessions(
            user,
            endSessions: request.Status == UserStatus.Disabled,
            () => this.auditService.Record(SecurityEventType.UserStatusChanged, actor, user.Id, user.UserName, actor.Id, user.BranchId, $"{previous} -> {request.Status}"),
            actor,
            cancellationToken);

        return Response.Succeed(ResponseMessages.Updated);
    }

    public async Task<Response> Unlock(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.FindManageable(id, actor, allowSelf: false, cancellationToken);

        if (denied != null)
        {
            return denied;
        }

        user!.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.UserUnlocked, actor, user.Id, user.UserName, actor.Id, user.BranchId);
        await this.unitOfWork.SaveChanges(cancellationToken);

        return Response.Succeed(ResponseMessages.UserUnlocked);
    }

    public async Task<Response> ResetPassword(Guid id, ResetPasswordRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        // Your own password goes through auth/password, which checks the current one.
        var (user, denied) = await this.FindManageable(id, actor, allowSelf: false, cancellationToken);

        if (denied != null)
        {
            return denied;
        }

        var passwordPolicy = await this.passwordPolicyRepository.GetCurrent(cancellationToken);

        if (!passwordPolicy.IsSatisfiedBy(request.NewPassword))
        {
            return Response.Fail(ResponseMessages.PasswordTooWeak);
        }

        user!.PasswordHash = this.passwordService.Hash(request.NewPassword);
        user.PasswordChangedAt = this.Now;
        user.MustChangePassword = request.MustChangePassword;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.SecurityStamp = Generator.NewStamp();
        user.ConcurrencyStamp = Generator.NewStamp();

        await this.SaveAndEndSessions(
            user,
            endSessions: true,
            () => this.auditService.Record(SecurityEventType.PasswordReset, actor, user.Id, user.UserName, actor.Id, user.BranchId),
            actor,
            cancellationToken);

        return Response.Succeed(ResponseMessages.PasswordReset);
    }

    public async Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.FindManageable(id, actor, allowSelf: false, cancellationToken);

        if (denied != null)
        {
            return denied;
        }

        // Soft delete: the row stays for the audit trail, the username and email stay reserved.
        user!.IsDeleted = true;
        user.DeletedAt = this.Now;
        user.DeletedBy = actor.Id;
        user.Status = UserStatus.Disabled;
        user.SecurityStamp = Generator.NewStamp();
        user.ConcurrencyStamp = Generator.NewStamp();

        // Frees the branch so the SuperAdmin can create its next admin.
        if (user.UserType == UserType.Admin)
        {
            var branch = await this.branchesRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.AdminUserId == user.Id, cancellationToken);

            if (branch != null)
            {
                branch.AdminUserId = null;
                branch.ConcurrencyStamp = Generator.NewStamp();
            }
        }

        await this.SaveAndEndSessions(
            user,
            endSessions: true,
            () => this.auditService.Record(SecurityEventType.UserDeleted, actor, user.Id, user.UserName, actor.Id, user.BranchId),
            actor,
            cancellationToken);

        return Response.Succeed(ResponseMessages.Deleted);
    }

    public async Task<Response<UserDetailsViewModel>> SetPermissions(Guid id, PermissionsRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var (user, denied) = await this.FindManageable(id, actor, allowSelf: false, cancellationToken, includePermissions: true);

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

        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);

        if (!AccessGuard.CanChangePermissions(actorAccess, added.Select(a => a.Code), removed.Select(a => a.Permission.Code)))
        {
            return Response<UserDetailsViewModel>.Forbidden(ResponseMessages.PrivilegeEscalation);
        }

        if (added.Count == 0 && removed.Count == 0)
        {
            return await this.GetDetails(id, cancellationToken);
        }

        var now = this.Now;

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

        return await this.GetDetails(id, cancellationToken);
    }

    private static List<string> NormaliseCodes(IEnumerable<string> codes)
    {
        return codes.Select(a => a.Trim()).Where(a => a.Length > 0).Distinct(StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Unscoped: callers have already checked the actor may see the user.
    /// </summary>
    private async Task<Response<UserDetailsViewModel>> GetDetails(Guid id, CancellationToken cancellationToken)
    {
        var user = await this.usersRepository.GetQueryable()
            .Include(a => a.Branch)
            .Include(a => a.UserPermissions).ThenInclude(a => a.Permission)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (user == null)
        {
            return Response<UserDetailsViewModel>.NotFound(ResponseMessages.UserNotFound);
        }

        var access = await this.accessProvider.Get(id, cancellationToken);

        return Response<UserDetailsViewModel>.Succeed(ViewModelFactory.CreateDetails(user, access, this.Now));
    }

    /// <summary>
    /// Loads a user the actor may see and manage. Out of scope reads as not found; in scope but
    /// above the actor in the hierarchy reads as forbidden.
    /// </summary>
    private async Task<(User? User, Response? Denied)> FindManageable(
        Guid id,
        RequestUser actor,
        bool allowSelf,
        CancellationToken cancellationToken,
        bool includePermissions = false)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var users = this.usersRepository.GetTrackedQueryable();

        if (includePermissions)
        {
            users = users.Include(a => a.UserPermissions).ThenInclude(a => a.Permission);
        }

        var user = await AccessGuard.VisibleTo(users, actorAccess).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (user == null)
        {
            return (null, Response.NotFound(ResponseMessages.UserNotFound));
        }

        if (user.Id == actor.Id)
        {
            return allowSelf ? (user, null) : (null, Response.Fail(ResponseMessages.CannotModifySelf));
        }

        return await this.accessGuard.CanManage(actorAccess, user, cancellationToken)
            ? (user, null)
            : (null, Response.Forbidden(ResponseMessages.CannotManageUser));
    }

    /// <summary>
    /// Callers that end sessions have already rotated the user's security stamp; saving it is
    /// what invalidates both access and refresh tokens. Marking the refresh tokens revoked
    /// afterwards only keeps the RefreshTokens table truthful.
    /// </summary>
    private async Task SaveAndEndSessions(User user, bool endSessions, Action recordAudit, RequestUser actor, CancellationToken cancellationToken)
    {
        recordAudit();
        await this.unitOfWork.SaveChanges(cancellationToken);
        await this.accessProvider.Invalidate(user.Id, cancellationToken);

        if (endSessions)
        {
            await this.refreshTokensRepository.RevokeAllActive(user.Id, this.Now, actor.IpAddress, AdminActionReason, cancellationToken);
        }
    }
}
