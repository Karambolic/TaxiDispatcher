namespace Domain.Interfaces;

public interface IRepository<T> where T : class
{
    T? GetById(int id);
    List<T> GetAll();
    void Add(T entity);
    bool Update(T entity);

    /// <summary>
    /// Implemented just to have full set of CRUD operations, but in this project no records will be deleted
    /// </summary>
    /// <param name="id">Id of the record to delete</param>
    /// <returns>True if the record was deleted, false otherwise</returns>
    bool Delete(int id);
}