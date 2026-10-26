namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// The holder of a digital identity card, as printed on it. Stored with the QR scan that returned
/// the card, in the same row.
/// </summary>
public class IdentityCard
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

    public string? CardNumber { get; set; }
}
