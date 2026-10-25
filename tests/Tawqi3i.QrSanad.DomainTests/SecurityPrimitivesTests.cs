namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Security;

public class SecurityPrimitivesTests
{
    private readonly PasswordPolicySettings policy = new();

    [TestCase("Str0ng!Pass#2026", ExpectedResult = true)]
    [TestCase("short1!A", ExpectedResult = false)]
    [TestCase("alllowercase1!", ExpectedResult = false)]
    [TestCase("ALLUPPERCASE1!", ExpectedResult = false)]
    [TestCase("NoDigitsHere!!", ExpectedResult = false)]
    [TestCase("NoSymbols12345", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(null, ExpectedResult = false)]
    public bool PasswordPolicy(string? password) => this.policy.IsSatisfiedBy(password);

    [Test]
    public void PasswordPolicy_RejectsOverlongPasswords()
    {
        Assert.That(this.policy.IsSatisfiedBy("Aa1!" + new string('x', 200)), Is.False);
    }

    [Test]
    public void DeterministicGuid_IsStableAndDistinct()
    {
        // Seed data keys come from this; if it changed, every migration would rewrite the seeds.
        Assert.That(Generator.CreateDeterministicGuid("a"), Is.EqualTo(Generator.CreateDeterministicGuid("a")));
        Assert.That(Generator.CreateDeterministicGuid("a"), Is.Not.EqualTo(Generator.CreateDeterministicGuid("b")));
    }

    [Test]
    public void RandomToken_IsUrlSafeAndUnique()
    {
        var first = Generator.RandomToken(64);
        var second = Generator.RandomToken(64);

        Assert.That(first, Does.Match("^[A-Za-z0-9_-]+$"));
        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void Normalizer_IsCaseAndWhitespaceInsensitive()
    {
        Assert.That(Normalizer.Normalize("  Admin.Amman "), Is.EqualTo(Normalizer.Normalize("ADMIN.AMMAN")));
    }
}
