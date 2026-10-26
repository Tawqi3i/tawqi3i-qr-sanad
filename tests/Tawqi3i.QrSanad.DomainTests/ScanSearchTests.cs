namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Audit;

public class ScanSearchTests
{
    private static readonly IQueryable<SecurityAuditLog> Scans = new[]
    {
        Scan("2000000001", "محمد احمد علي", "MOHAMMAD AHMAD ALI"),
        Scan("2000000002", "بلال سامي خالد", "BILAL SAMI KHALED"),
        new SecurityAuditLog { Id = Guid.NewGuid() } // a failed scan or another document
    }.AsQueryable();

    [TestCase("2000000001", "2000000001")]
    [TestCase("200000000", "2000000001,2000000002")]
    [TestCase("احمد", "2000000001")]
    [TestCase("sami", "2000000002")]
    [TestCase("  بلال ", "2000000002")]
    public void FindsByNationalNumberOrName(string search, string expected)
    {
        var found = SecurityAuditLogsService.Search(Scans, search).Select(a => a.IdentityCard!.NationalNumber);

        Assert.That(string.Join(',', found), Is.EqualTo(expected));
    }

    [Test]
    public void MatchesTheNationalNumberFromItsStartOnly()
    {
        Assert.That(SecurityAuditLogsService.Search(Scans, "0001").Count(), Is.Zero);
    }

    [TestCase(null)]
    [TestCase(" ")]
    public void ReturnsEveryScanWithoutASearch(string? search)
    {
        Assert.That(SecurityAuditLogsService.Search(Scans, search).Count(), Is.EqualTo(3));
    }

    private static SecurityAuditLog Scan(string nationalNumber, string arabicName, string englishName)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            IdentityCard = new IdentityCard { NationalNumber = nationalNumber, ArabicName = arabicName, EnglishName = englishName }
        };
    }
}
