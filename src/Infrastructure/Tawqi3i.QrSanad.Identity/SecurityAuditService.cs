namespace Tawqi3i.QrSanad.Identity;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Persistence;

internal sealed class SecurityAuditService : ISecurityAuditService
{
    private readonly ISecurityAuditLogRepository repository;
    private readonly TimeProvider timeProvider;

    public SecurityAuditService(ISecurityAuditLogRepository repository, TimeProvider timeProvider)
    {
        this.repository = repository;
        this.timeProvider = timeProvider;
    }

    public void Record(
        SecurityEventType eventType,
        ClientInfo client,
        Guid? userId = null,
        string? userName = null,
        Guid? actorId = null,
        Guid? branchId = null,
        string? details = null,
        IdentityCard? identityCard = null)
    {
        this.repository.AddOnly(new SecurityAuditLog
        {
            Id = Generator.CreateV7Guid(),
            EventType = eventType,
            UserId = userId,
            UserName = Normalizer.Truncate(userName, 256),
            ActorId = actorId,
            BranchId = branchId,
            IpAddress = Normalizer.Truncate(client.IpAddress, 64),
            UserAgent = Normalizer.Truncate(client.UserAgent, 512),
            Details = Normalizer.Truncate(details, 2000),
            IdentityCard = identityCard,
            CreatedAt = this.timeProvider.GetUtcNow().UtcDateTime
        });
    }
}
