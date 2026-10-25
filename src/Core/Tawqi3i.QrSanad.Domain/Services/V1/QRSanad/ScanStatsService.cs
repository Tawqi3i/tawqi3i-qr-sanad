namespace Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// Loads the scans of the range (a few small columns, the range capped at <see cref="MaxRangeDays"/>)
/// and counts them in memory, so the daily buckets follow the viewer's time zone and the
/// queries stay identical on every database provider.
/// </summary>
internal sealed class ScanStatsService : IScanStatsService
{
    internal const int MaxRangeDays = 92;
    internal const int DefaultRangeDays = 30;
    internal const int TopUsersCount = 5;

    private readonly ISecurityAuditLogRepository auditLogRepository;
    private readonly IBranchesRepository branchesRepository;
    private readonly AccessGuard accessGuard;
    private readonly TimeProvider timeProvider;

    public ScanStatsService(
        ISecurityAuditLogRepository auditLogRepository,
        IBranchesRepository branchesRepository,
        AccessGuard accessGuard,
        TimeProvider timeProvider)
    {
        this.auditLogRepository = auditLogRepository;
        this.branchesRepository = branchesRepository;
        this.accessGuard = accessGuard;
        this.timeProvider = timeProvider;
    }

    public async Task<Response<ScanStatsViewModel>> Get(ScanStatsQuery query, RequestUser actor, CancellationToken cancellationToken = default)
    {
        var offset = TimeSpan.FromMinutes(query.UtcOffsetMinutes);
        var to = query.To ?? this.timeProvider.GetUtcNow().UtcDateTime;
        var from = query.From ?? DefaultFrom(to, offset);

        if (from >= to || to - from > TimeSpan.FromDays(MaxRangeDays))
        {
            return Response<ScanStatsViewModel>.Fail(ResponseMessages.ScanStatsRangeInvalid);
        }

        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);

        var scans = (await AccessGuard.VisibleTo(this.auditLogRepository.GetQueryable(), actorAccess)
                .Where(a => (a.EventType == SecurityEventType.QrScanSucceeded || a.EventType == SecurityEventType.QrScanFailed)
                    && a.CreatedAt >= from
                    && a.CreatedAt < to)
                .Select(a => new { a.CreatedAt, a.EventType, a.UserId, a.UserName, a.BranchId })
                .ToListAsync(cancellationToken))
            .Select(a => new Scan(a.CreatedAt, a.EventType == SecurityEventType.QrScanSucceeded, a.UserId, a.UserName, a.BranchId))
            .ToList();

        var stats = Summarise(scans, from, to, offset);
        stats.Branches = actorAccess.IsSuperAdmin ? await this.CountBranches(scans, cancellationToken) : null;

        return Response<ScanStatsViewModel>.Succeed(stats);
    }

    /// <summary>
    /// The start of the viewer's day, <see cref="DefaultRangeDays"/> days back including today.
    /// </summary>
    internal static DateTime DefaultFrom(DateTime to, TimeSpan offset) => (to + offset).Date.AddDays(-(DefaultRangeDays - 1)) - offset;

    /// <summary>
    /// Totals, one entry per local day of the range (empty days included) and the top users.
    /// </summary>
    internal static ScanStatsViewModel Summarise(IReadOnlyCollection<Scan> scans, DateTime from, DateTime to, TimeSpan offset)
    {
        var byDay = scans.ToLookup(a => LocalDay(a.CreatedAt, offset));
        var lastDay = LocalDay(to.AddTicks(-1), offset);
        var daily = new List<DailyScanCountsViewModel>();

        for (var day = LocalDay(from, offset); day <= lastDay; day = day.AddDays(1))
        {
            daily.Add(CountInto(new DailyScanCountsViewModel { Date = day }, byDay[day]));
        }

        var stats = CountInto(new ScanStatsViewModel { From = from, To = to }, scans);
        stats.Daily = daily;
        stats.TopUsers = scans
            .Where(a => a.UserId != null)
            .GroupBy(a => a.UserId!.Value)
            .Select(g => CountInto(new UserScanCountsViewModel { UserId = g.Key, UserName = g.First().UserName }, g))
            .OrderByDescending(a => a.Total)
            .ThenBy(a => a.UserName, StringComparer.OrdinalIgnoreCase)
            .Take(TopUsersCount)
            .ToList();

        return stats;
    }

    private static DateOnly LocalDay(DateTime utc, TimeSpan offset) => DateOnly.FromDateTime(utc + offset);

    private static T CountInto<T>(T model, IEnumerable<Scan> scans)
        where T : ScanCountsViewModel
    {
        foreach (var scan in scans)
        {
            if (scan.Succeeded)
            {
                model.Succeeded++;
            }
            else
            {
                model.Failed++;
            }
        }

        return model;
    }

    private async Task<List<BranchScanCountsViewModel>> CountBranches(List<Scan> scans, CancellationToken cancellationToken)
    {
        var byBranch = scans.Where(a => a.BranchId != null).GroupBy(a => a.BranchId!.Value).ToList();
        var branchIds = byBranch.Select(g => g.Key).ToList();

        var names = await this.branchesRepository.GetQueryable()
            .Where(a => branchIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        return byBranch
            .Select(g => CountInto(new BranchScanCountsViewModel { BranchId = g.Key, BranchName = names.GetValueOrDefault(g.Key) }, g))
            .OrderByDescending(a => a.Total)
            .ThenBy(a => a.BranchName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal sealed record Scan(DateTime CreatedAt, bool Succeeded, Guid? UserId, string? UserName, Guid? BranchId);
}
