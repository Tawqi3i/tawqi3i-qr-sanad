namespace Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// CreatedAt/CreatedBy and UpdatedAt/UpdatedBy are stamped by QrSanadSqlContext.SaveChanges.
/// Bulk ExecuteUpdate calls bypass the change tracker, so they must set UpdatedAt themselves.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}
