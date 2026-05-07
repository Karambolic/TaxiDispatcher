using Domain.Entities;

namespace Domain.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Client? GetByPhone(string phone);
}