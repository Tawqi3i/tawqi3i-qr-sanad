namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// The password rules, set by the SuperAdmin. A single row with <see cref="SingletonId"/>,
/// created at startup from Identity:PasswordPolicy when the table is empty.
/// </summary>
public class PasswordPolicy : AuditableEntity
{
    public static readonly Guid SingletonId = new("6b3c1f0e-2d4a-4f7b-9e1c-5a8d7c2b4e90");

    public int MinLength { get; set; }

    public int MaxLength { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireNonAlphanumeric { get; set; }
}
