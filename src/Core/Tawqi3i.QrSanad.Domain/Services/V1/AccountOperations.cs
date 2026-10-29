namespace Tawqi3i.QrSanad.Domain.Services.V1;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

/// <summary>
/// What admins and devices have in common: listing, the checks before creating an account, and
/// the administrative changes. Callers decide the scope (which accounts the actor may reach)
/// and pass in an account already found within it.
/// </summary>
internal sealed class AccountOperations
{
    private const string AdminActionReason = "admin_action";

    private readonly IUsersRepository usersRepository;
    private readonly IBranchesRepository branchesRepository;
    private readonly IRefreshTokensRepository refreshTokensRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPasswordService passwordService;
    private readonly IUserAccessProvider accessProvider;
    private readonly ISecurityAuditService auditService;
    private readonly IPasswordPolicyRepository passwordPolicyRepository;
    private readonly TimeProvider timeProvider;

    public AccountOperations(
        IUsersRepository usersRepository,
        IBranchesRepository branchesRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IUserAccessProvider accessProvider,
        ISecurityAuditService auditService,
        IPasswordPolicyRepository passwordPolicyRepository,
        TimeProvider timeProvider)
    {
        this.usersRepository = usersRepository;
        this.branchesRepository = branchesRepository;
        this.refreshTokensRepository = refreshTokensRepository;
        this.unitOfWork = unitOfWork;
        this.passwordService = passwordService;
        this.accessProvider = accessProvider;
        this.auditService = auditService;
        this.passwordPolicyRepository = passwordPolicyRepository;
        this.timeProvider = timeProvider;
    }

    public DateTime Now => this.timeProvider.GetUtcNow().UtcDateTime;

    public async Task<PagedResponse<UserViewModel>> GetPage(IQueryable<User> accounts, UsersQuery query, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = Normalizer.Normalize(query.Search);
            accounts = accounts.Where(a =>
                a.NormalizedUserName.Contains(term)
                || (a.NormalizedEmail != null && a.NormalizedEmail.Contains(term))
                || a.DisplayName.ToUpper().Contains(term));
        }

        if (query.Status is { } status)
        {
            accounts = accounts.Where(a => a.Status == status);
        }

        if (query.BranchId is { } branchId)
        {
            accounts = accounts.Where(a => a.BranchId == branchId);
        }

        var totalCount = await accounts.CountAsync(cancellationToken);

        var items = await accounts
            .Include(a => a.Branch)
            .OrderByDescending(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return ViewModelFactory.CreatePaginated(items, totalCount, query, this.Now);
    }

    /// <summary>
    /// Unscoped: callers have already checked the actor may see the account.
    /// </summary>
    public async Task<Response<UserDetailsViewModel>> GetDetails(Guid id, CancellationToken cancellationToken)
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
    /// Loads an account from the actor's scope, tracked. Out of scope reads as not found.
    /// </summary>
    public async Task<(User? User, Response? Denied)> Find(
        IQueryable<User> scope,
        Guid id,
        RequestUser actor,
        bool allowSelf,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var user = await scope.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (user == null)
        {
            return (null, Response.NotFound(notFoundMessage));
        }

        return user.Id == actor.Id && !allowSelf
            ? (null, Response.Fail(ResponseMessages.CannotModifySelf))
            : (user, null);
    }

    /// <summary>
    /// The branch a new account goes to, tracked, provided it exists and is active.
    /// </summary>
    public async Task<(Branch? Branch, Response? Denied)> GetActiveBranch(Guid branchId, CancellationToken cancellationToken)
    {
        var branch = await this.branchesRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == branchId, cancellationToken);

        if (branch == null)
        {
            return (null, Response.NotFound(ResponseMessages.BranchNotFound));
        }

        return branch.Status == BranchStatus.Active
            ? (branch, null)
            : (null, Response.Fail(ResponseMessages.BranchInactive));
    }

    /// <summary>
    /// The password meets the policy and the username, and the email when there is one, are free.
    /// </summary>
    public async Task<Response?> ValidateNewAccount(string userName, string? email, string password, CancellationToken cancellationToken)
    {
        var passwordPolicy = await this.passwordPolicyRepository.GetCurrent(cancellationToken);

        if (!passwordPolicy.IsSatisfiedBy(password))
        {
            return Response.Fail(ResponseMessages.PasswordTooWeak);
        }

        if (await this.usersRepository.UserNameExists(Normalizer.Normalize(userName), cancellationToken: cancellationToken))
        {
            return Response.Conflict(ResponseMessages.UserNameTaken);
        }

        if (email != null && await this.usersRepository.EmailExists(Normalizer.Normalize(email), cancellationToken: cancellationToken))
        {
            return Response.Conflict(ResponseMessages.EmailTaken);
        }

        return null;
    }

    public string HashPassword(string password) => this.passwordService.Hash(password);

    /// <summary>
    /// Adds the new account and returns its details once saved.
    /// </summary>
    public async Task<Response<UserDetailsViewModel>> Add(User user, Branch branch, RequestUser actor, CancellationToken cancellationToken)
    {
        this.usersRepository.AddOnly(user);
        this.auditService.Record(SecurityEventType.UserCreated, actor, user.Id, user.UserName, actor.Id, branch.Id, $"{user.UserType} of branch {branch.Name}");
        await this.unitOfWork.SaveChanges(cancellationToken);

        var created = await this.GetDetails(user.Id, cancellationToken);
        return Response<UserDetailsViewModel>.Created(created.Data!);
    }

    /// <summary>
    /// Saves profile changes the caller has made to the account.
    /// </summary>
    public async Task<Response<UserDetailsViewModel>> SaveUpdate(User user, RequestUser actor, CancellationToken cancellationToken)
    {
        user.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.UserUpdated, actor, user.Id, user.UserName, actor.Id, user.BranchId);
        await this.unitOfWork.SaveChanges(cancellationToken);

        return await this.GetDetails(user.Id, cancellationToken);
    }

    public async Task<Response> SetStatus(User user, UserStatusRequest request, RequestUser actor, CancellationToken cancellationToken)
    {
        if (user.Status == request.Status)
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

    public async Task<Response> Unlock(User user, RequestUser actor, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.UserUnlocked, actor, user.Id, user.UserName, actor.Id, user.BranchId);
        await this.unitOfWork.SaveChanges(cancellationToken);

        return Response.Succeed(ResponseMessages.UserUnlocked);
    }

    public async Task<Response> ResetPassword(User user, ResetPasswordRequest request, RequestUser actor, CancellationToken cancellationToken)
    {
        var passwordPolicy = await this.passwordPolicyRepository.GetCurrent(cancellationToken);

        if (!passwordPolicy.IsSatisfiedBy(request.NewPassword))
        {
            return Response.Fail(ResponseMessages.PasswordTooWeak);
        }

        user.PasswordHash = this.passwordService.Hash(request.NewPassword);
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

    public async Task<Response> Delete(User user, RequestUser actor, CancellationToken cancellationToken)
    {
        // Soft delete: the row stays for the audit trail, the username and email stay reserved.
        user.IsDeleted = true;
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

    /// <summary>
    /// Callers that end sessions have already rotated the account's security stamp; saving it is
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
