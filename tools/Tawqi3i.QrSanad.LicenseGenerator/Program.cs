// Issues QrSanad license keys. See README.md next to this file.
//
// Quick use: edit the values below and Run (F5). The license key is printed in the console.

using System.Globalization;
using System.Security.Cryptography;
using Tawqi3i.QrSanad.Licensing;

// ============================== Edit these values, then Run ==============================

const string PrivateKeyPath = @"D:\QRSanad\Licensing\private.pem";

var license = new LicenseData
{
    Customer = "Customer name",
    Type = "TRIAL",                                    // TRIAL, PROD or DEV
    IssuedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.Date.AddDays(5),
    MaxUsers = 50,                                     // null = unlimited (the SuperAdmin is not counted)
    MaxBranches = 50                                    // null = unlimited
};

// ==========================================================================================

const string Usage = """
    Without arguments, issues the license defined at the top of Program.cs.

    Commands:
      keygen <output-directory>
          Creates a new key pair: writes private.pem and prints the public key to put in
          LicensePublicKey.cs. A new pair invalidates every license issued with the old one.

      issue --private-key <file> --customer <name> --type <TRIAL|PROD|DEV>
            (--days <n> | --expires <yyyy-MM-dd>) [--max-users <n>] [--max-branches <n>]
          Prints a signed license key for Licensing:Key or PUT /api/v1/license.

      verify <license-key>
          Checks a key against the public key built into the API and prints what it grants.
    """;

try
{
    return args switch
    {
        [] => Issue(license, PrivateKeyPath),
        ["keygen", var directory] => KeyGen(directory),
        ["issue", .. var options] => IssueFromOptions(ParseOptions(options)),
        ["verify", var key] => Verify(key),
        _ => Fail(Usage)
    };
}
catch (Exception exception) when (exception is ArgumentException or FormatException or IOException or CryptographicException)
{
    return Fail(exception.Message);
}

static int Issue(LicenseData license, string privateKeyPath)
{
    if (license.Type is not ("TRIAL" or "PROD" or "DEV"))
    {
        throw new ArgumentException("Type must be TRIAL, PROD or DEV.");
    }

    using var ecdsa = ECDsa.Create();
    ecdsa.ImportFromPem(File.ReadAllText(privateKeyPath));

    var payload = LicenseKeyFormat.Serialize(license);
    var key = LicenseKeyFormat.Encode(payload, ecdsa.SignData(payload, HashAlgorithmName.SHA256));

    if (LicenseVerifier.CreateDefault().Verify(key) == null)
    {
        return Fail("This private key does not match the public key built into the API, so the API would reject the license.");
    }

    Console.WriteLine($"License for {license}");
    Console.WriteLine();
    Console.WriteLine("==================================== License Key ====================================");
    Console.WriteLine(key);
    Console.WriteLine("======================================================================================");

    return 0;
}

static int IssueFromOptions(Dictionary<string, string> options)
{
    var now = DateTime.UtcNow;

    var license = new LicenseData
    {
        Customer = Required(options, "customer"),
        Type = Required(options, "type").ToUpperInvariant(),
        IssuedAt = now,
        ExpiresAt = options.TryGetValue("expires", out var expires)
            ? DateTime.SpecifyKind(DateTime.ParseExact(expires, "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc)
            : now.Date.AddDays(int.Parse(Required(options, "days"), CultureInfo.InvariantCulture)),
        MaxUsers = OptionalInt(options, "max-users"),
        MaxBranches = OptionalInt(options, "max-branches")
    };

    return Issue(license, Required(options, "private-key"));
}

static int KeyGen(string directory)
{
    var path = Path.Combine(directory, "private.pem");

    if (File.Exists(path))
    {
        return Fail($"{path} already exists. Move it away first: replacing it invalidates every issued license.");
    }

    Directory.CreateDirectory(directory);

    using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    File.WriteAllText(path, ecdsa.ExportPkcs8PrivateKeyPem());

    Console.WriteLine($"Private key written to {path}. Keep it secret and out of source control.");
    Console.WriteLine("Public key for LicensePublicKey.Value:");
    Console.WriteLine(Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo()));

    return 0;
}

static int Verify(string key)
{
    var license = LicenseVerifier.CreateDefault().Verify(key);

    if (license == null)
    {
        return Fail("Invalid: malformed, tampered with, or signed with a different private key.");
    }

    Console.WriteLine($"Valid: {license}");
    Console.WriteLine(license.IsExpired(DateTime.UtcNow) ? "Status: expired" : $"Status: active, issued {license.IssuedAt:yyyy-MM-dd}");

    return 0;
}

static Dictionary<string, string> ParseOptions(string[] options)
{
    if (options.Length % 2 != 0)
    {
        throw new ArgumentException("Every option needs a value.");
    }

    var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < options.Length; i += 2)
    {
        if (!options[i].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Unexpected argument '{options[i]}'.");
        }

        parsed[options[i][2..]] = options[i + 1];
    }

    return parsed;
}

static string Required(Dictionary<string, string> options, string name) =>
    options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException($"--{name} is required.");

static int? OptionalInt(Dictionary<string, string> options, string name) =>
    options.TryGetValue(name, out var value) ? int.Parse(value, CultureInfo.InvariantCulture) : null;

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
