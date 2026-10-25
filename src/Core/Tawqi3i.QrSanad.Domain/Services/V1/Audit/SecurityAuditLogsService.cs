namespace Tawqi3i.QrSanad.Domain.Services.V1.Audit;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;

internal sealed class SecurityAuditLogsService : ISecurityAuditLogsService
{
    private readonly ISecurityAuditLogRepository repository;
    private readonly AccessGuard accessGuard;

    public SecurityAuditLogsService(ISecurityAuditLogRepository repository, AccessGuard accessGuard)
    {
        this.repository = repository;
        this.accessGuard = accessGuard;
    }

    public async Task<PagedResponse<SecurityAuditLogViewModel>> GetAll(SecurityAuditLogsQuery query, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var logs = AccessGuard.VisibleTo(this.repository.GetQueryable(), actorAccess);

        if (query.UserId is { } userId)
        {
            logs = logs.Where(a => a.UserId == userId || a.ActorId == userId);
        }

        if (query.EventType is { } eventType)
        {
            logs = logs.Where(a => a.EventType == eventType);
        }

        if (query.From is { } from)
        {
            logs = logs.Where(a => a.CreatedAt >= from);
        }

        if (query.To is { } to)
        {
            logs = logs.Where(a => a.CreatedAt < to);
        }

        var totalCount = await logs.CountAsync(cancellationToken);

        var items = await logs
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResponse<SecurityAuditLogViewModel>.Ok(items.Select(ViewModelFactory.Create).ToList(), totalCount, query);
    }
}
