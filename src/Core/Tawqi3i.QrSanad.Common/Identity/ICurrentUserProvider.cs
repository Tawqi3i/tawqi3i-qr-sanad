namespace Tawqi3i.QrSanad.Common.Identity;

/// <summary>
/// The authenticated caller, if any. Used to stamp CreatedBy/UpdatedBy on saved entities.
/// </summary>
public interface ICurrentUserProvider
{
    Guid? UserId { get; }
}
