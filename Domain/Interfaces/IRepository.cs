namespace Domain.Interfaces;

public interface IRepository<T> where T : class
{
    T? GetById(int id);
    List<T> GetAll();
    void Add(T entity);
    bool Update(T entity);
    bool Delete(int id);
}