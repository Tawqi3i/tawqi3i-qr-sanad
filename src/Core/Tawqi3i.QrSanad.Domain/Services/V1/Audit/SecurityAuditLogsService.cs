namespace Tawqi3i.QrSanad.Domain.Services.V1.Audit;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Audit;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;
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
        var logs = Filter(AccessGuard.VisibleTo(this.repository.GetQueryable(), actorAccess), query);

        if (query.EventType is { } eventType)
        {
            logs = logs.Where(a => a.EventType == eventType);
        }

        return await Page(logs, query, cancellationToken);
    }

    public async Task<PagedResponse<SecurityAuditLogViewModel>> GetScans(ScansQuery query, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var scans = Filter(AccessGuard.ScansVisibleTo(this.repository.GetQueryable(), actorAccess), query);

        return await Page(Search(scans, query.Search), query, cancellationToken);
    }

    /// <summary>
    /// Digits match the national number from its start; anything else is part of either name. The
    /// English names are stored in upper case, as printed on the card.
    /// </summary>
    internal static IQueryable<SecurityAuditLog> Search(IQueryable<SecurityAuditLog> scans, string? search)
    {
        var text = search?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return scans;
        }

        if (text.All(char.IsAsciiDigit))
        {
            return scans.Where(a => a.IdentityCard != null && a.IdentityCard.NationalNumber.StartsWith(text));
        }

        var englishText = text.ToUpperInvariant();
        return scans.Where(a => a.IdentityCard != null
            && (a.IdentityCard.ArabicName.Contains(text) || a.IdentityCard.EnglishName.Contains(englishText)));
    }

    private static IQueryable<SecurityAuditLog> Filter(IQueryable<SecurityAuditLog> logs, AuditLogsFilter filter)
    {
        if (filter.UserId is { } userId)
        {
            logs = logs.Where(a => a.UserId == userId || a.ActorId == userId);
        }

        if (filter.From is { } from)
        {
            logs = logs.Where(a => a.CreatedAt >= from);
        }

        if (filter.To is { } to)
        {
            logs = logs.Where(a => a.CreatedAt < to);
        }

        return logs;
    }

    private static async Task<PagedResponse<SecurityAuditLogViewModel>> Page(IQueryable<SecurityAuditLog> logs, PagedQuery query, CancellationToken cancellationToken)
    {
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
