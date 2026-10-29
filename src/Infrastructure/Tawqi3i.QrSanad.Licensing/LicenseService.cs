namespace Tawqi3i.QrSanad.Licensing;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Licensing;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// The installed key is read and its signature checked on every call, so the license cannot be
/// extended by editing the database. The checks guard rare operations, so this costs little.
/// </summary>
internal sealed class LicenseService : ILicenseService
{
    private readonly ILicenseRepository licenseRepository;
    private readonly IUsersRepository usersRepository;
    private readonly IBranchesRepository branchesRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ISecurityAuditService auditService;
    private readonly LicenseVerifier verifier;
    private readonly LicensingSettings settings;
    private readonly TimeProvider timeProvider;

    public LicenseService(
        ILicenseRepository licenseRepository,
        IUsersRepository usersRepository,
        IBranchesRepository branchesRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        LicenseVerifier verifier,
        LicensingSettings settings,
        TimeProvider timeProvider)
    {
        this.licenseRepository = licenseRepository;
        this.usersRepository = usersRepository;
        this.branchesRepository = branchesRepository;
        this.unitOfWork = unitOfWork;
        this.auditService = auditService;
        this.verifier = verifier;
        this.settings = settings;
        this.timeProvider = timeProvider;
    }

    private DateTime Now => this.timeProvider.GetUtcNow().UtcDateTime;

    public async Task<Response<LicenseViewModel>> Get(CancellationToken cancellationToken = default)
    {
        var license = await this.GetInstalled(cancellationToken);

        return license == null
            ? Response<LicenseViewModel>.NotFound(ResponseMessages.LicenseMissing)
            : Response<LicenseViewModel>.Succeed(await this.ToViewModel(license, cancellationToken));
    }

    public async Task<Response<LicenseViewModel>> Update(LicenseRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var license = this.verifier.Verify(request.Key);

        if (license == null)
        {
            return Response<LicenseViewModel>.Fail(ResponseMessages.LicenseInvalid);
        }

        if (license.IsExpired(this.Now))
        {
            return Response<LicenseViewModel>.Fail(ResponseMessages.LicenseExpired);
        }

        await this.licenseRepository.StageKey(request.Key.Trim(), cancellationToken);
        this.auditService.Record(SecurityEventType.LicenseUpdated, actor, actorId: actor.Id, details: license.ToString());
        await this.unitOfWork.SaveChanges(cancellationToken);

        return Response<LicenseViewModel>.Succeed(await this.ToViewModel(license, cancellationToken), ResponseMessages.LicenseUpdated);
    }

    public async Task<Response?> EnsureActive(CancellationToken cancellationToken = default)
    {
        var license = await this.GetInstalled(cancellationToken);

        return this.CheckActive(license);
    }

    public Task<Response?> EnsureCanAddUser(CancellationToken cancellationToken = default) =>
        this.EnsureRoom(a => a.MaxUsers, this.CountUsers, ResponseMessages.LicenseUserLimitReached, cancellationToken);

    public Task<Response?> EnsureCanAddDevice(CancellationToken cancellationToken = default) =>
        this.EnsureRoom(a => a.MaxDevices, this.CountDevices, ResponseMessages.LicenseDeviceLimitReached, cancellationToken);

    public Task<Response?> EnsureCanAddBranch(CancellationToken cancellationToken = default) =>
        this.EnsureRoom(a => a.MaxBranches, this.CountBranches, ResponseMessages.LicenseBranchLimitReached, cancellationToken);

    /// <summary>
    /// The stored key, or the configured one if nothing valid is stored yet.
    /// </summary>
    private async Task<LicenseData?> GetInstalled(CancellationToken cancellationToken)
    {
        var storedKey = await this.licenseRepository.GetStoredKey(cancellationToken);

        return this.verifier.Verify(storedKey) ?? this.verifier.Verify(this.settings.Key);
    }

    private Response? CheckActive(LicenseData? license)
    {
        if (license == null)
        {
            return Response.Forbidden(ResponseMessages.LicenseMissing);
        }

        return license.IsExpired(this.Now) ? Response.Forbidden(ResponseMessages.LicenseExpired) : null;
    }

    private async Task<Response?> EnsureRoom(
        Func<LicenseData, int?> limit,
        Func<CancellationToken, Task<int>> count,
        string limitReachedMessage,
        CancellationToken cancellationToken)
    {
        var license = await this.GetInstalled(cancellationToken);

        if (this.CheckActive(license) is { } denied)
        {
            return denied;
        }

        return limit(license!) is { } max && await count(cancellationToken) >= max
            ? Response.Forbidden(limitReachedMessage)
            : null;
    }

    private Task<int> CountUsers(CancellationToken cancellationToken) =>
        this.usersRepository.GetQueryable().CountAsync(a => a.UserType == UserType.Admin, cancellationToken);

    private Task<int> CountDevices(CancellationToken cancellationToken) =>
        this.usersRepository.GetQueryable().CountAsync(a => a.UserType == UserType.Device, cancellationToken);

    private Task<int> CountBranches(CancellationToken cancellationToken) =>
        this.branchesRepository.GetQueryable().CountAsync(cancellationToken);

    private async Task<LicenseViewModel> ToViewModel(LicenseData license, CancellationToken cancellationToken)
    {
        var remaining = license.ExpiresAt - this.Now;

        return new LicenseViewModel
        {
            Customer = license.Customer,
            Type = license.Type,
            IssuedAt = license.IssuedAt,
            ExpiresAt = license.ExpiresAt,
            IsExpired = license.IsExpired(this.Now),
            DaysRemaining = Math.Max(0, (int)remaining.TotalDays),
            MaxUsers = license.MaxUsers,
            UsersCount = await this.CountUsers(cancellationToken),
            MaxDevices = license.MaxDevices,
            DevicesCount = await this.CountDevices(cancellationToken),
            MaxBranches = license.MaxBranches,
            BranchesCount = await this.CountBranches(cancellationToken)
        };
    }
}
