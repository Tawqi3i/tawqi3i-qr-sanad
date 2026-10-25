namespace Tawqi3i.QrSanad.DomainTests;

using System.Security.Cryptography;
using Tawqi3i.QrSanad.Licensing;

public class LicenseVerifierTests
{
    private static readonly LicenseData License = new()
    {
        Customer = "Test Customer",
        Type = "TRIAL",
        IssuedAt = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
        ExpiresAt = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        MaxUsers = 10,
        MaxBranches = 2
    };

    private ECDsa signingKey = null!;
    private LicenseVerifier verifier = null!;

    [SetUp]
    public void SetUp()
    {
        this.signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        this.verifier = new LicenseVerifier(this.signingKey.ExportSubjectPublicKeyInfo());
    }

    [TearDown]
    public void TearDown() => this.signingKey.Dispose();

    [Test]
    public void AcceptsAKeySignedWithTheMatchingPrivateKey()
    {
        Assert.That(this.verifier.Verify(this.Issue(License)), Is.EqualTo(License));
    }

    [Test]
    public void RejectsATamperedPayload()
    {
        // The genuine signature, attached to a payload that grants more users.
        var genuine = this.Issue(License);
        var inflated = this.Issue(License with { MaxUsers = 10_000 });
        var forged = inflated[..inflated.IndexOf('.')] + genuine[genuine.IndexOf('.')..];

        Assert.That(this.verifier.Verify(forged), Is.Null);
    }

    [Test]
    public void RejectsAKeySignedWithAnotherPrivateKey()
    {
        using var otherKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var payload = LicenseKeyFormat.Serialize(License);
        var key = LicenseKeyFormat.Encode(payload, otherKey.SignData(payload, HashAlgorithmName.SHA256));

        Assert.That(this.verifier.Verify(key), Is.Null);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("TSS-LIC-abc.def")]
    [TestCase("QRS-LIC-no-signature")]
    [TestCase("QRS-LIC-***.***")]
    public void RejectsMalformedKeys(string? key)
    {
        Assert.That(this.verifier.Verify(key), Is.Null);
    }

    [Test]
    public void IgnoresSurroundingWhitespace()
    {
        Assert.That(this.verifier.Verify($"  {this.Issue(License)}\n"), Is.EqualTo(License));
    }

    [Test]
    public void ConfiguredLicense_ReplacesTheStoredOneWhenIssuedLater_EvenIfItGrantsLess()
    {
        var unlimitedDev = License with { Type = "DEV", ExpiresAt = License.ExpiresAt.AddYears(5), MaxBranches = null };
        var restrictedTrial = License with { IssuedAt = License.IssuedAt.AddDays(1), MaxBranches = 2 };

        Assert.That(LicenseInitializer.ReplacesStored(restrictedTrial, stored: unlimitedDev), Is.True);
        Assert.That(LicenseInitializer.ReplacesStored(unlimitedDev, stored: restrictedTrial), Is.False);
        Assert.That(LicenseInitializer.ReplacesStored(License, stored: null), Is.True);
        Assert.That(LicenseInitializer.ReplacesStored(null, stored: License), Is.False);
    }

    [Test]
    public void IsExpired_FromTheExpiryInstantOn()
    {
        Assert.That(License.IsExpired(License.ExpiresAt.AddTicks(-1)), Is.False);
        Assert.That(License.IsExpired(License.ExpiresAt), Is.True);
    }

    private string Issue(LicenseData license)
    {
        var payload = LicenseKeyFormat.Serialize(license);
        return LicenseKeyFormat.Encode(payload, this.signingKey.SignData(payload, HashAlgorithmName.SHA256));
    }
}
