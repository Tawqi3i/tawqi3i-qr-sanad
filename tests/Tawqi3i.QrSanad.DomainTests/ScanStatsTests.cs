namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;
using Scan = Tawqi3i.QrSanad.Domain.Services.V1.QRSanad.ScanStatsService.Scan;

public class ScanStatsTests
{
    private static readonly TimeSpan Amman = TimeSpan.FromHours(3);
    private static readonly DateTime From = new(2026, 10, 19, 21, 0, 0, DateTimeKind.Utc); // 20 Oct 00:00 in Amman
    private static readonly DateTime To = new(2026, 10, 22, 21, 0, 0, DateTimeKind.Utc);   // 23 Oct 00:00 in Amman

    [Test]
    public void Daily_HasOneEntryPerLocalDayIncludingEmptyDays()
    {
        var stats = ScanStatsService.Summarise([], From, To, Amman);

        Assert.That(stats.Daily.Select(a => a.Date), Is.EqualTo(new[] { new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21), new DateOnly(2026, 10, 22) }));
        Assert.That(stats.Daily.All(a => a.Total == 0), Is.True);
    }

    [Test]
    public void Daily_BucketsByTheViewersDayNotUtc()
    {
        // 22:30 UTC on the 20th is 01:30 on the 21st in Amman.
        var stats = ScanStatsService.Summarise([Scan(new DateTime(2026, 10, 20, 22, 30, 0, DateTimeKind.Utc), true)], From, To, Amman);

        Assert.That(stats.Daily.Single(a => a.Date == new DateOnly(2026, 10, 21)).Succeeded, Is.EqualTo(1));
        Assert.That(stats.Daily.Single(a => a.Date == new DateOnly(2026, 10, 20)).Total, Is.Zero);
    }

    [Test]
    public void Totals_SplitSucceededAndFailed()
    {
        var at = From.AddHours(5);
        var stats = ScanStatsService.Summarise([Scan(at, true), Scan(at, true), Scan(at, false)], From, To, Amman);

        Assert.That(stats.Succeeded, Is.EqualTo(2));
        Assert.That(stats.Failed, Is.EqualTo(1));
        Assert.That(stats.Total, Is.EqualTo(3));
    }

    [Test]
    public void TopUsers_AreOrderedByTotalAndCapped()
    {
        var at = From.AddHours(5);
        var busiest = Guid.NewGuid();
        var scans = new List<Scan> { Scan(at, true, busiest, "busiest"), Scan(at, false, busiest, "busiest") };

        for (var i = 0; i < ScanStatsService.TopUsersCount + 2; i++)
        {
            scans.Add(Scan(at, true, Guid.NewGuid(), $"user{i}"));
        }

        var stats = ScanStatsService.Summarise(scans, From, To, Amman);

        Assert.That(stats.TopUsers, Has.Count.EqualTo(ScanStatsService.TopUsersCount));
        Assert.That(stats.TopUsers[0].UserName, Is.EqualTo("busiest"));
        Assert.That(stats.TopUsers[0].Failed, Is.EqualTo(1));
    }

    [Test]
    public void DefaultFrom_StartsAtTheViewersMidnightThirtyDaysBack()
    {
        var now = new DateTime(2026, 10, 24, 22, 0, 0, DateTimeKind.Utc); // 25 Oct 01:00 in Amman

        var from = ScanStatsService.DefaultFrom(now, Amman);

        Assert.That(from, Is.EqualTo(new DateTime(2026, 9, 25, 21, 0, 0))); // 26 Sep 00:00 in Amman
        Assert.That(ScanStatsService.Summarise([], from, now, Amman).Daily, Has.Count.EqualTo(ScanStatsService.DefaultRangeDays));
    }

    private static Scan Scan(DateTime at, bool succeeded, Guid? userId = null, string? userName = null) =>
        new(at, succeeded, userId, userName, null);
}
