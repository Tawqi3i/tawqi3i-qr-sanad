namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

public class UserPermission
{
    public Guid UserId { get; set; }

    public Guid PermissionId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid? AssignedBy { get; set; }

    public User User { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
