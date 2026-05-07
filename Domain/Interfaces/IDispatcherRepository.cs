using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IDispatcherRepository : IRepository<Dispatcher>
    {
        public string? GetHashedPasswordByLogin(string login);

        public Dispatcher? GetByLogin(string login);
    }
}
