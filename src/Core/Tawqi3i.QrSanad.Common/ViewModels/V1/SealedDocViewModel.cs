namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

public class SealedDocViewModel
{
    /// <summary>
    /// Base64 contents of the sealed file.
    /// </summary>
    public string SignedFile { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// The holder, read from a digital identity card. Null for any other document, or when the card could not be read.
    /// </summary>
    public IdentityCardViewModel? IdentityCard { get; set; }
}

/// <summary>
/// The text of the card as printed. Every field except the national number and the names is null
/// when it is missing from the card.
/// </summary>
public class IdentityCardViewModel
{
    public string NationalNumber { get; set; } = string.Empty;

    public string ArabicName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public string? BloodType { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? MotherName { get; set; }

    public string? RegistryNumber { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? Residence { get; set; }

    /// <summary>
    /// The number of the card itself (رقم الهوية), not the national number.
    /// </summary>
    public string? CardNumber { get; set; }
}
