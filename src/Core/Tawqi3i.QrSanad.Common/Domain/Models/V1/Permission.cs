namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// A row per code in PermissionCodes.All, seeded by the migrations. Not editable through the API.
/// </summary>
public class Permission
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
