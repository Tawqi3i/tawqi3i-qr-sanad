namespace Tawqi3i.QrSanad.Licensing;

public class LicensingSettings
{
    /// <summary>
    /// A license key from the license generator. Installed at startup when it was issued after
    /// the installed one; the SuperAdmin can also install keys through the API.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
