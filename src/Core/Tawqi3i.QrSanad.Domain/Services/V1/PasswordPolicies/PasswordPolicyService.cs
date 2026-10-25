namespace Tawqi3i.QrSanad.Domain.Services.V1.PasswordPolicies;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

internal sealed class PasswordPolicyService : IPasswordPolicyService
{
    private readonly IPasswordPolicyRepository passwordPolicyRepository;
    private readonly IUsersRepository usersRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserAccessProvider accessProvider;
    private readonly ISecurityAuditService auditService;
    private readonly TimeProvider timeProvider;

    public PasswordPolicyService(
        IPasswordPolicyRepository passwordPolicyRepository,
        IUsersRepository usersRepository,
        IUnitOfWork unitOfWork,
        IUserAccessProvider accessProvider,
        ISecurityAuditService auditService,
        TimeProvider timeProvider)
    {
        this.passwordPolicyRepository = passwordPolicyRepository;
        this.usersRepository = usersRepository;
        this.unitOfWork = unitOfWork;
        this.accessProvider = accessProvider;
        this.auditService = auditService;
        this.timeProvider = timeProvider;
    }

    public async Task<Response<PasswordPolicyViewModel>> Get(CancellationToken cancellationToken = default)
    {
        var policy = await this.passwordPolicyRepository.GetCurrent(cancellationToken);

        return Response<PasswordPolicyViewModel>.Succeed(ViewModelFactory.Create(policy));
    }

    public async Task<Response<PasswordPolicyViewModel>> Update(PasswordPolicyRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        if (request.MaxLength < request.MinLength)
        {
            return Response<PasswordPolicyViewModel>.Fail(ResponseMessages.PasswordPolicyLengthInvalid);
        }

        var previous = await this.passwordPolicyRepository.GetCurrent(cancellationToken);
        var updated = DomainModelFactory.CreatePasswordPolicy(request);
        var now = this.timeProvider.GetUtcNow().UtcDateTime;

        await this.unitOfWork.ExecuteInTransaction(
            async token =>
            {
                var entity = await this.GetOrAddEntity(token);
                updated.ApplyTo(entity);

                var flagged = await this.usersRepository.RequirePasswordChangeForAll(now, actor.Id, token);

                this.auditService.Record(
                    SecurityEventType.PasswordPolicyUpdated,
                    actor,
                    actorId: actor.Id,
                    details: $"{previous} -> {updated}. {flagged} user(s) must change their password");

                await this.unitOfWork.SaveChanges(token);
            },
            cancellationToken);

        // The flag is checked against the cached access snapshot; drop it so it applies on
        // everyone's very next request.
        await this.accessProvider.Invalidate(await this.usersRepository.GetAllIds(cancellationToken), cancellationToken);

        return Response<PasswordPolicyViewModel>.Succeed(ViewModelFactory.Create(updated), ResponseMessages.PasswordPolicyUpdated);
    }

    private async Task<PasswordPolicy> GetOrAddEntity(CancellationToken cancellationToken)
    {
        var entity = await this.passwordPolicyRepository.GetTrackedQueryable().FirstOrDefaultAsync(a => a.Id == PasswordPolicy.SingletonId, cancellationToken);

        if (entity == null)
        {
            entity = new PasswordPolicy { Id = PasswordPolicy.SingletonId };
            this.passwordPolicyRepository.AddOnly(entity);
        }

        return entity;
    }
}
