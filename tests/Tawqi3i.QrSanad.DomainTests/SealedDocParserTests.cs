namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Sanad;

public class SealedDocParserTests
{
    [Test]
    public void ReadsTheDocumentInsideTheDataArray()
    {
        const string body = """
            { "data": [ { "status": 200, "message": "OK", "data": { "signedFile": { "fileContents": "JVBERi0x", "contentType": "application/pdf" } } } ] }
            """;

        var result = SealedDocParser.Parse(body, 200);

        Assert.That(result!.FileContents, Is.EqualTo("JVBERi0x"));
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
    }

    [Test]
    public void ReadsAnUnwrappedDocumentAndDefaultsTheContentType()
    {
        const string body = """{ "data": { "signedFile": { "fileContents": "JVBERi0x" } } }""";

        var result = SealedDocParser.Parse(body, 200);

        Assert.That(result!.FileContents, Is.EqualTo("JVBERi0x"));
        Assert.That(result.ContentType, Is.EqualTo(SealedDocParser.DefaultContentType));
    }

    [Test]
    public void PrefersMoDeesOwnStatusAndMessage()
    {
        const string body = """{ "data": [ { "status": 400, "message": "QR code not found" } ] }""";

        var result = SealedDocParser.Parse(body, 200);

        Assert.That(result!.FileContents, Is.Null);
        Assert.That(result.Status, Is.EqualTo(400));
        Assert.That(result.Message, Is.EqualTo("QR code not found"));
    }

    [Test]
    public void FallsBackToTheHttpStatus()
    {
        var result = SealedDocParser.Parse("""{ "data": [] }""", 503);

        Assert.That(result!.Status, Is.EqualTo(503));
        Assert.That(result.FileContents, Is.Null);
    }

    [Test]
    public void ReturnsNullForANonJsonBody()
    {
        Assert.That(SealedDocParser.Parse("<html>Bad gateway</html>", 502), Is.Null);
    }
}
