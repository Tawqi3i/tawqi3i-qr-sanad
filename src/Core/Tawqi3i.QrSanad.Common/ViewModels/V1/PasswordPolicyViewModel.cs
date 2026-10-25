namespace Tawqi3i.QrSanad.Common.ViewModels.V1;

public class PasswordPolicyViewModel
{
    public int MinLength { get; set; }

    public int MaxLength { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireNonAlphanumeric { get; set; }
}
