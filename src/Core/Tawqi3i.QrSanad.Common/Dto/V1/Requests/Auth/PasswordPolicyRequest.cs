namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;

using System.ComponentModel.DataAnnotations;

public class PasswordPolicyRequest
{
    /// <summary>
    /// Every password field accepts at most this many characters, so the policy cannot allow more.
    /// </summary>
    public const int MaxAllowedLength = 128;

    public const int MinAllowedLength = 8;

    [Range(MinAllowedLength, MaxAllowedLength, ErrorMessage = "The {0} field must be between {1} and {2}")]
    public int MinLength { get; set; }

    [Range(MinAllowedLength, MaxAllowedLength, ErrorMessage = "The {0} field must be between {1} and {2}")]
    public int MaxLength { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireNonAlphanumeric { get; set; }
}
