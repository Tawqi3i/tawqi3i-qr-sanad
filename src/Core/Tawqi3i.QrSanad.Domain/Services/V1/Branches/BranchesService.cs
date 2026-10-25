namespace Tawqi3i.QrSanad.Domain.Services.V1.Branches;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

internal sealed class BranchesService : IBranchesService
{
    private const string BranchDeletedReason = "branch_deleted";

    private readonly IBranchesRepository branchesRepository;
    private readonly IUsersRepository usersRepository;
    private readonly IRefreshTokensRepository refreshTokensRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserAccessProvider accessProvider;
    private readonly ISecurityAuditService auditService;
    private readonly AccessGuard accessGuard;
    private readonly ILicenseService licenseService;
    private readonly TimeProvider timeProvider;

    public BranchesService(
        IBranchesRepository branchesRepository,
        IUsersRepository usersRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IUnitOfWork unitOfWork,
        IUserAccessProvider accessProvider,
        ISecurityAuditService auditService,
        AccessGuard accessGuard,
        ILicenseService licenseService,
        TimeProvider timeProvider)
    {
        this.licenseService = licenseService;
        this.branchesRepository = branchesRepository;
        this.usersRepository = usersRepository;
        this.refreshTokensRepository = refreshTokensRepository;
        this.unitOfWork = unitOfWork;
        this.accessProvider = accessProvider;
        this.auditService = auditService;
        this.accessGuard = accessGuard;
        this.timeProvider = timeProvider;
    }

    public async Task<PagedResponse<BranchViewModel>> GetAll(BranchesQuery query, CancellationToken cancellationToken = default)
    {
        var branches = this.branchesRepository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = Normalizer.Normalize(query.Search);
            branches = branches.Where(a => a.NormalizedName.Contains(term) || (a.Code != null && a.Code.ToUpper().Contains(term)));
        }

        if (query.Status is { } status)
        {
            branches = branches.Where(a => a.Status == status);
        }

        var totalCount = await branches.CountAsync(cancellationToken);

        var items = await Project(branches
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Id)
                .Skip(query.Skip)
                .Take(query.PageSize))
            .ToListAsync(cancellationToken);

        return PagedResponse<BranchViewModel>.Ok(items.Select(a => ViewModelFactory.Create(a.Branch, a.Admin, a.UsersCount)).ToList(), totalCount, query);
    }

    public async Task<Response<BranchViewModel>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await Project(this.branchesRepository.GetQueryable().Where(a => a.Id == id)).FirstOrDefaultAsync(cancellationToken);

        return item == null
            ? Response<BranchViewModel>.NotFound(ResponseMessages.BranchNotFound)
            : Response<BranchViewModel>.Succeed(ViewModelFactory.Create(item.Branch, item.Admin, item.UsersCount));
    }

    public async Task<Response<BranchViewModel>> GetCurrent(RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);

        return actorAccess.BranchId is { } branchId
            ? await this.Get(branchId, cancellationToken)
            : Response<BranchViewModel>.NotFound(ResponseMessages.BranchNotFound);
    }

    public async Task<Response<BranchViewModel>> Create(BranchRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        if (await this.licenseService.EnsureCanAddBranch(cancellationToken) is { } licenseDenied)
        {
            return Response<BranchViewModel>.From(licenseDenied);
        }

        var conflict = await this.CheckUnique(request, null, cancellationToken);

        if (conflict != null)
        {
            return Response<BranchViewModel>.From(conflict);
        }

        var branch = DomainModelFactory.CreateBranch(request, actor.Id);

        this.branchesRepository.AddOnly(branch);
        this.auditService.Record(SecurityEventType.BranchCreated, actor, actorId: actor.Id, branchId: branch.Id, details: branch.Name);
        await this.unitOfWork.SaveChanges(cancellationToken);

        var created = await this.Get(branch.Id, cancellationToken);
        return Response<BranchViewModel>.Created(created.Data!);
    }

    public async Task<Response<BranchViewModel>> Update(Guid id, BranchRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var branch = await this.branchesRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (branch == null)
        {
            return Response<BranchViewModel>.NotFound(ResponseMessages.BranchNotFound);
        }

        var conflict = await this.CheckUnique(request, id, cancellationToken);

        if (conflict != null)
        {
            return Response<BranchViewModel>.From(conflict);
        }

        var previousName = branch.Name;
        var name = request.Name.Trim();

        branch.Name = name;
        branch.NormalizedName = Normalizer.Normalize(name);
        branch.Code = DomainModelFactory.NullIfBlank(request.Code);
        branch.Address = DomainModelFactory.NullIfBlank(request.Address);
        branch.PhoneNumber = DomainModelFactory.NullIfBlank(request.PhoneNumber);
        branch.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.BranchUpdated, actor, actorId: actor.Id, branchId: branch.Id, details: $"{previousName} -> {branch.Name}");
        await this.unitOfWork.SaveChanges(cancellationToken);

        return await this.Get(id, cancellationToken);
    }

    public async Task<Response> SetStatus(Guid id, BranchStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var branch = await this.branchesRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (branch == null)
        {
            return Response.NotFound(ResponseMessages.BranchNotFound);
        }

        if (branch.Status == request.Status)
        {
            return Response.Succeed(ResponseMessages.Updated);
        }

        var previous = branch.Status;
        branch.Status = request.Status;
        branch.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.BranchStatusChanged, actor, actorId: actor.Id, branchId: branch.Id, details: $"{branch.Name}: {previous} -> {request.Status}");
        await this.unitOfWork.SaveChanges(cancellationToken);

        // Every member's cached access includes the branch status; drop it so the change
        // applies on their very next request.
        await this.accessProvider.Invalidate(await this.usersRepository.GetIdsInBranch(id, cancellationToken), cancellationToken);

        return Response.Succeed(ResponseMessages.Updated);
    }

    public async Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var branch = await this.branchesRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (branch == null)
        {
            return Response.NotFound(ResponseMessages.BranchNotFound);
        }

        var now = this.timeProvider.GetUtcNow().UtcDateTime;

        branch.IsDeleted = true;
        branch.DeletedAt = now;
        branch.DeletedBy = actor.Id;
        branch.Status = BranchStatus.Disabled;
        branch.ConcurrencyStamp = Generator.NewStamp();

        this.auditService.Record(SecurityEventType.BranchDeleted, actor, actorId: actor.Id, branchId: branch.Id, details: branch.Name);
        await this.unitOfWork.SaveChanges(cancellationToken);

        // The members can no longer sign in (their branch is deleted); this ends their sessions too.
        await this.accessProvider.Invalidate(await this.usersRepository.GetIdsInBranch(id, cancellationToken), cancellationToken);
        await this.refreshTokensRepository.RevokeAllActiveInBranch(id, now, actor.IpAddress, BranchDeletedReason, cancellationToken);

        return Response.Succeed(ResponseMessages.Deleted);
    }

    private static IQueryable<BranchProjection> Project(IQueryable<Branch> branches)
    {
        return branches.Select(a => new BranchProjection
        {
            Branch = a,
            Admin = a.Admin,
            UsersCount = a.Users.Count(u => !u.IsDeleted)
        });
    }

    private async Task<Response?> CheckUnique(BranchRequest request, Guid? excludeBranchId, CancellationToken cancellationToken)
    {
        if (await this.branchesRepository.NameExists(Normalizer.Normalize(request.Name), excludeBranchId, cancellationToken))
        {
            return Response.Conflict(ResponseMessages.BranchNameTaken);
        }

        var code = DomainModelFactory.NullIfBlank(request.Code);

        if (code != null && await this.branchesRepository.CodeExists(code, excludeBranchId, cancellationToken))
        {
            return Response.Conflict(ResponseMessages.BranchCodeTaken);
        }

        return null;
    }

    private sealed class BranchProjection
    {
        public Branch Branch { get; init; } = null!;

        public User? Admin { get; init; }

        public int UsersCount { get; init; }
    }
}
