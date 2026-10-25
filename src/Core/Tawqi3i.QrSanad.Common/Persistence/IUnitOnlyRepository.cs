namespace Tawqi3i.QrSanad.Common.Persistence;

/// <summary>
/// Stages changes without saving; IUnitOfWork.SaveChanges persists them together.
/// </summary>
public interface IUnitOnlyRepository<T>
    where T : class
{
    void AddOnly(T model);

    void AddOnlyRange(IEnumerable<T> models);

    void DeleteOnly(T model);

    void DeleteOnlyRange(IEnumerable<T> models);
}
