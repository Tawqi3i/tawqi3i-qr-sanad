namespace Tawqi3i.QrSanad.Identity.Services;

using System.Net;
using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// Users are read without tracking and changed with set-based updates: login counters are
/// incremented atomically in SQL, so concurrent guesses cannot overwrite each other's count.
/// </summary>
internal sealed class AuthService : IAuthService
{
    private const string RotatedReason = "rotated";
    private const string LogoutReason = "logout";
    private const string LogoutAllReason = "logout_all";
    private const string PasswordChangedReason = "password_changed";
    private const string ReuseDetectedReason = "reuse_detected";
    private const string UserInactiveReason = "user_inactive";

    private readonly IUsersRepository usersRepository;
    private readonly IRefreshTokensRepository refreshTokensRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPasswordService passwordService;
    private readonly ITokenProvider tokenProvider;
    private readonly IUserAccessProvider accessProvider;
    private readonly ISecurityAuditService auditService;
    private readonly AuthSettings authSettings;
    private readonly IPasswordPolicyRepository passwordPolicyRepository;
    private readonly TimeProvider timeProvider;

    public AuthService(
        IUsersRepository usersRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ITokenProvider tokenProvider,
        IUserAccessProvider accessProvider,
        ISecurityAuditService auditService,
        AuthSettings authSettings,
        IPasswordPolicyRepository passwordPolicyRepository,
        TimeProvider timeProvider)
    {
        this.usersRepository = usersRepository;
        this.refreshTokensRepository = refreshTokensRepository;
        this.unitOfWork = unitOfWork;
        this.passwordService = passwordService;
        this.tokenProvider = tokenProvider;
        this.accessProvider = accessProvider;
        this.auditService = auditService;
        this.authSettings = authSettings;
        this.passwordPolicyRepository = passwordPolicyRepository;
        this.timeProvider = timeProvider;
    }

    private DateTime Now => this.timeProvider.GetUtcNow().UtcDateTime;

    public async Task<Response<TokenResponse>> Login(LoginRequest request, ClientInfo client, CancellationToken cancellationToken = default)
    {
        var now = this.Now;
        var user = await this.usersRepository.FindByUserNameOrEmail(Normalizer.Normalize(request.UserNameOrEmail), cancellationToken);

        if (user == null)
        {
            // Same cost as a real check, so response time does not reveal which usernames exist.
            this.passwordService.VerifyAgainstDummy(request.Password);
            this.auditService.Record(SecurityEventType.LoginFailed, client, userName: request.UserNameOrEmail, details: "Unknown user");
            await this.unitOfWork.SaveChanges(cancellationToken);

            return Response<TokenResponse>.Unauthorized(ResponseMessages.InvalidCredentials);
        }

        if (user.LockoutEnd > now)
        {
            this.auditService.Record(SecurityEventType.LoginFailed, client, user.Id, user.UserName, branchId: user.BranchId, details: "Locked out");
            await this.unitOfWork.SaveChanges(cancellationToken);

            return Response<TokenResponse>.Fail(ResponseMessages.AccountLocked, HttpStatusCode.Locked);
        }

        var check = this.passwordService.Verify(user.PasswordHash, request.Password);

        if (check == PasswordCheck.Failed)
        {
            await this.RegisterFailedAttempt(user, client, now, cancellationToken);
            return Response<TokenResponse>.Unauthorized(ResponseMessages.InvalidCredentials);
        }

        // Checked only after the password, so the response does not reveal that a disabled
        // account exists to someone who does not know its password.
        var refusal = GetInactiveReason(user);

        if (refusal != null)
        {
            this.auditService.Record(SecurityEventType.LoginFailed, client, user.Id, user.UserName, branchId: user.BranchId, details: refusal);
            await this.unitOfWork.SaveChanges(cancellationToken);

            return Response<TokenResponse>.Forbidden(refusal);
        }

        var upgradedHash = check == PasswordCheck.SuccessRehashNeeded ? this.passwordService.Hash(request.Password) : null;
        TokenResponse? tokens = null;

        await this.unitOfWork.ExecuteInTransaction(
            async token =>
            {
                await this.usersRepository.Update(
                    a => a.Id == user.Id,
                    setters =>
                    {
                        setters
                            .SetProperty(a => a.AccessFailedCount, 0)
                            .SetProperty(a => a.LockoutEnd, (DateTime?)null)
                            .SetProperty(a => a.LastLoginAt, now);

                        if (upgradedHash != null)
                        {
                            setters.SetProperty(a => a.PasswordHash, upgradedHash);
                        }
                    },
                    token);

                tokens = this.IssueTokens(user, client, now, Generator.CreateV7Guid());
                this.auditService.Record(SecurityEventType.LoginSucceeded, client, user.Id, user.UserName, branchId: user.BranchId);
                await this.unitOfWork.SaveChanges(token);
            },
            cancellationToken);

        return Response<TokenResponse>.Succeed(tokens!);
    }

    public async Task<Response<TokenResponse>> Refresh(RefreshTokenRequest request, ClientInfo client, CancellationToken cancellationToken = default)
    {
        var now = this.Now;
        var stored = await this.refreshTokensRepository.FindByHash(this.tokenProvider.HashRefreshToken(request.RefreshToken), cancellationToken);

        if (stored == null || stored.ExpiresAt <= now)
        {
            return Response<TokenResponse>.Unauthorized(ResponseMessages.InvalidRefreshToken);
        }

        if (stored.RevokedAt != null)
        {
            // A token that was already exchanged is being presented again: either it was stolen
            // or the client replayed it. Either way, end every session of that user.
            if (stored.RevokeReason == RotatedReason)
            {
                await this.EndAllSessions(stored.UserId, client, now, ReuseDetectedReason, SecurityEventType.RefreshTokenReuseDetected, cancellationToken);
            }

            return Response<TokenResponse>.Unauthorized(ResponseMessages.InvalidRefreshToken);
        }

        var user = await this.usersRepository.GetQueryable()
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == stored.UserId, cancellationToken);

        // A changed stamp means the password was reset or sessions were ended since issue.
        if (user == null || GetInactiveReason(user) != null || !string.Equals(user.SecurityStamp, stored.SecurityStamp, StringComparison.Ordinal))
        {
            await this.refreshTokensRepository.TryRevoke(stored.Id, now, client.IpAddress, UserInactiveReason, cancellationToken: cancellationToken);
            return Response<TokenResponse>.Unauthorized(ResponseMessages.InvalidRefreshToken);
        }

        TokenResponse? tokens = null;

        await this.unitOfWork.ExecuteInTransaction(
            async token =>
            {
                var replacementId = Generator.CreateV7Guid();

                // Claim the old token first. If a concurrent refresh got there first, this one
                // loses and issues nothing.
                if (!await this.refreshTokensRepository.TryRevoke(stored.Id, now, client.IpAddress, RotatedReason, replacementId, token))
                {
                    return;
                }

                // The replacement keeps the original expiry: rotation does not extend a session.
                tokens = this.IssueTokens(user, client, now, replacementId, stored.ExpiresAt);
                await this.unitOfWork.SaveChanges(token);
            },
            cancellationToken);

        return tokens == null
            ? Response<TokenResponse>.Unauthorized(ResponseMessages.InvalidRefreshToken)
            : Response<TokenResponse>.Succeed(tokens);
    }

    public async Task<Response> Logout(RefreshTokenRequest request, RequestUser user, CancellationToken cancellationToken = default)
    {
        var stored = await this.refreshTokensRepository.FindByHash(this.tokenProvider.HashRefreshToken(request.RefreshToken), cancellationToken);

        // Another user's token is ignored rather than reported, so this cannot probe for tokens.
        if (stored != null && stored.UserId == user.Id)
        {
            await this.refreshTokensRepository.TryRevoke(stored.Id, this.Now, user.IpAddress, LogoutReason, cancellationToken: cancellationToken);
        }

        var access = await this.accessProvider.Get(user.Id, cancellationToken);

        this.auditService.Record(SecurityEventType.Logout, user, user.Id, branchId: access?.BranchId);
        await this.unitOfWork.SaveChanges(cancellationToken);

        return Response.Succeed(ResponseMessages.LoggedOut);
    }

    public async Task<Response> LogoutAll(RequestUser user, CancellationToken cancellationToken = default)
    {
        await this.EndAllSessions(user.Id, user, this.Now, LogoutAllReason, SecurityEventType.LogoutAll, cancellationToken);

        return Response.Succeed(ResponseMessages.LoggedOut);
    }

    public async Task<Response<TokenResponse>> ChangePassword(ChangePasswordRequest request, RequestUser user, CancellationToken cancellationToken = default)
    {
        var now = this.Now;
        var entity = await this.usersRepository.GetQueryable().FirstOrDefaultAsync(a => a.Id == user.Id, cancellationToken);

        if (entity == null)
        {
            return Response<TokenResponse>.Unauthorized();
        }

        // Counts towards lockout like a failed login: a stolen access token must not allow
        // unlimited guessing of the current password.
        if (this.passwordService.Verify(entity.PasswordHash, request.CurrentPassword) == PasswordCheck.Failed)
        {
            await this.RegisterFailedAttempt(entity, user, now, cancellationToken);
            return Response<TokenResponse>.Fail(ResponseMessages.CurrentPasswordIncorrect);
        }

        var passwordPolicy = await this.passwordPolicyRepository.GetCurrent(cancellationToken);

        if (!passwordPolicy.IsSatisfiedBy(request.NewPassword))
        {
            return Response<TokenResponse>.Fail(ResponseMessages.PasswordTooWeak);
        }

        if (string.Equals(request.NewPassword, request.CurrentPassword, StringComparison.Ordinal))
        {
            return Response<TokenResponse>.Fail(ResponseMessages.PasswordSameAsCurrent);
        }

        entity.PasswordHash = this.passwordService.Hash(request.NewPassword);
        entity.SecurityStamp = Generator.NewStamp();
        entity.MustChangePassword = false;

        TokenResponse? tokens = null;

        await this.unitOfWork.ExecuteInTransaction(
            async token =>
            {
                await this.usersRepository.Update(
                    a => a.Id == entity.Id,
                    setters => setters
                        .SetProperty(a => a.PasswordHash, entity.PasswordHash)
                        .SetProperty(a => a.SecurityStamp, entity.SecurityStamp)
                        .SetProperty(a => a.ConcurrencyStamp, Generator.NewStamp())
                        .SetProperty(a => a.MustChangePassword, false)
                        .SetProperty(a => a.PasswordChangedAt, now)
                        .SetProperty(a => a.AccessFailedCount, 0)
                        .SetProperty(a => a.UpdatedAt, now)
                        .SetProperty(a => a.UpdatedBy, entity.Id),
                    token);

                // Every other session ends; this one continues with the tokens returned below.
                await this.refreshTokensRepository.RevokeAllActive(entity.Id, now, user.IpAddress, PasswordChangedReason, token);

                tokens = this.IssueTokens(entity, user, now, Generator.CreateV7Guid());
                this.auditService.Record(SecurityEventType.PasswordChanged, user, entity.Id, entity.UserName, branchId: entity.BranchId);
                await this.unitOfWork.SaveChanges(token);
            },
            cancellationToken);

        await this.accessProvider.Invalidate(entity.Id, cancellationToken);

        return Response<TokenResponse>.Succeed(tokens!, ResponseMessages.PasswordChanged);
    }

    public async Task<Response<ProfileViewModel>> GetProfile(RequestUser user, CancellationToken cancellationToken = default)
    {
        var entity = await this.usersRepository.GetQueryable()
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == user.Id, cancellationToken);

        var access = await this.accessProvider.Get(user.Id, cancellationToken);

        if (entity == null || access == null)
        {
            return Response<ProfileViewModel>.Unauthorized();
        }

        return Response<ProfileViewModel>.Succeed(new ProfileViewModel
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Email = entity.Email,
            DisplayName = entity.DisplayName,
            PhoneNumber = entity.PhoneNumber,
            Type = entity.UserType,
            BranchId = entity.BranchId,
            BranchName = entity.Branch?.Name,
            MustChangePassword = entity.MustChangePassword,
            LastLoginAt = entity.LastLoginAt,
            PasswordChangedAt = entity.PasswordChangedAt,
            Permissions = access.Permissions
        });
    }

    /// <summary>
    /// Why the user may not sign in, or null. Expects the branch to be loaded.
    /// </summary>
    private static string? GetInactiveReason(User user)
    {
        if (user.Status != UserStatus.Active)
        {
            return ResponseMessages.AccountDisabled;
        }

        if (user.Branch is { } branch && (branch.Status != BranchStatus.Active || branch.IsDeleted))
        {
            return ResponseMessages.BranchInactive;
        }

        return null;
    }

    private async Task RegisterFailedAttempt(User user, ClientInfo client, DateTime now, CancellationToken cancellationToken)
    {
        var maxAttempts = this.authSettings.MaxFailedAccessAttempts;
        DateTime? lockUntil = now.Add(this.authSettings.LockoutDuration);

        // LockoutEnd is assigned before AccessFailedCount on purpose: MySQL evaluates SET
        // clauses left to right against already-updated values, the other providers against the
        // original row. In this order both read the original count.
        await this.usersRepository.Update(
            a => a.Id == user.Id,
            setters => setters
                .SetProperty(a => a.LockoutEnd, a => a.AccessFailedCount + 1 >= maxAttempts ? lockUntil : a.LockoutEnd)
                .SetProperty(a => a.AccessFailedCount, a => a.AccessFailedCount + 1 >= maxAttempts ? 0 : a.AccessFailedCount + 1),
            cancellationToken);

        this.auditService.Record(SecurityEventType.LoginFailed, client, user.Id, user.UserName, branchId: user.BranchId, details: "Invalid password");

        if (user.AccessFailedCount + 1 >= maxAttempts)
        {
            this.auditService.Record(SecurityEventType.AccountLocked, client, user.Id, user.UserName, branchId: user.BranchId, details: $"Locked until {lockUntil:O}");
        }

        await this.unitOfWork.SaveChanges(cancellationToken);
    }

    private async Task EndAllSessions(Guid userId, ClientInfo client, DateTime now, string reason, SecurityEventType eventType, CancellationToken cancellationToken)
    {
        var newStamp = Generator.NewStamp();
        var branchId = await this.usersRepository.GetQueryable().Where(a => a.Id == userId).Select(a => a.BranchId).FirstOrDefaultAsync(cancellationToken);

        await this.unitOfWork.ExecuteInTransaction(
            async token =>
            {
                await this.refreshTokensRepository.RevokeAllActive(userId, now, client.IpAddress, reason, token);

                // A new security stamp invalidates every access token already issued.
                await this.usersRepository.Update(a => a.Id == userId, setters => setters.SetProperty(a => a.SecurityStamp, newStamp), token);

                this.auditService.Record(eventType, client, userId, branchId: branchId);
                await this.unitOfWork.SaveChanges(token);
            },
            cancellationToken);

        await this.accessProvider.Invalidate(userId, cancellationToken);
    }

    private TokenResponse IssueTokens(User user, ClientInfo client, DateTime now, Guid refreshTokenId, DateTime? refreshExpiresAt = null)
    {
        var accessToken = this.tokenProvider.CreateAccessToken(user, now);
        var refreshToken = this.tokenProvider.CreateRefreshToken();
        var expiresAt = refreshExpiresAt ?? now.Add(this.authSettings.RefreshTokenLifetime);

        this.refreshTokensRepository.AddOnly(new RefreshToken
        {
            Id = refreshTokenId,
            UserId = user.Id,
            TokenHash = refreshToken.Hash,
            SecurityStamp = user.SecurityStamp,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            CreatedByIp = Normalizer.Truncate(client.IpAddress, 64),
            UserAgent = Normalizer.Truncate(client.UserAgent, 512)
        });

        return new TokenResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = expiresAt,
            MustChangePassword = user.MustChangePassword
        };
    }
}
