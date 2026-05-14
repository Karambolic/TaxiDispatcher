namespace Domain.Interfaces;

public interface IRepository<T> where T : class
{
    T? GetById(int id);
    List<T> GetAll();
    void Add(T entity);
    bool Update(T entity);

    /// <summary>
    /// Implemented just to have full CRUD operations, but in this project no record will be deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool Delete(int id);
}