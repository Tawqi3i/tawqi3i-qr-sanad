namespace Tawqi3i.QrSanad.Common.Security;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// The password rules. Bound from Identity:PasswordPolicy as the initial values; the policy in
/// force is the stored <see cref="PasswordPolicy"/> row, which the SuperAdmin edits.
/// </summary>
public class PasswordPolicySettings
{
    public int MinLength { get; set; } = 10;

    public int MaxLength { get; set; } = 128;

    public bool RequireUppercase { get; set; } = true;

    public bool RequireLowercase { get; set; } = true;

    public bool RequireDigit { get; set; } = true;

    public bool RequireNonAlphanumeric { get; set; } = true;

    public static PasswordPolicySettings From(PasswordPolicy policy)
    {
        return new PasswordPolicySettings
        {
            MinLength = policy.MinLength,
            MaxLength = policy.MaxLength,
            RequireUppercase = policy.RequireUppercase,
            RequireLowercase = policy.RequireLowercase,
            RequireDigit = policy.RequireDigit,
            RequireNonAlphanumeric = policy.RequireNonAlphanumeric
        };
    }

    public void ApplyTo(PasswordPolicy policy)
    {
        policy.MinLength = this.MinLength;
        policy.MaxLength = this.MaxLength;
        policy.RequireUppercase = this.RequireUppercase;
        policy.RequireLowercase = this.RequireLowercase;
        policy.RequireDigit = this.RequireDigit;
        policy.RequireNonAlphanumeric = this.RequireNonAlphanumeric;
    }

    /// <summary>
    /// A one-line summary, for the audit log.
    /// </summary>
    public override string ToString() =>
        $"length {this.MinLength}-{this.MaxLength}, uppercase {this.RequireUppercase}, lowercase {this.RequireLowercase}, " +
        $"digit {this.RequireDigit}, symbol {this.RequireNonAlphanumeric}";

    public bool IsSatisfiedBy(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < this.MinLength || password.Length > this.MaxLength)
        {
            return false;
        }

        if (this.RequireUppercase && !password.Any(char.IsUpper))
        {
            return false;
        }

        if (this.RequireLowercase && !password.Any(char.IsLower))
        {
            return false;
        }

        if (this.RequireDigit && !password.Any(char.IsDigit))
        {
            return false;
        }

        if (this.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
        {
            return false;
        }

        return true;
    }
}
