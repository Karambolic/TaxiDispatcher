using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure;


namespace BusinessLogic.Services;

public class ClientService(UnitOfWork unitOfWork)
{
    public Client? GetClientByPhone(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
        return unitOfWork.Clients.GetByPhone(phoneNumber);
    }

    public Client GetOrCreateClient(string firstName, string lastName, string phoneNumber)
    {
        Client? existingClient = unitOfWork.Clients.GetByPhone(phoneNumber);
        if (existingClient != null) return existingClient;

        Client? newClient = new Client(firstName, lastName, phoneNumber);
        unitOfWork.Clients.Add(newClient);
        return newClient;
    }

    public List<Order> GetClientOrderHistory(int clientId)
    {
        return unitOfWork.Orders.GetAll()
            .Where(o => o.Client.Id == clientId)
            .OrderByDescending(o => o.CreatedAt).ToList();
    }

    public bool UpdateClientInfo(Client client) => unitOfWork.Clients.Update(client);

    public List<Client> GetAllClients() => unitOfWork.Clients.GetAll();

    /// <summary>
    /// Get clients whose phone numbers start with the specified mask
    /// </summary>
    public List<Client> GetClientsByPhoneMask(string mask)
    {
        if (string.IsNullOrWhiteSpace(mask)) return new List<Client>();
        return ((ClientRepository)unitOfWork.Clients).GetClientsByPhoneMask(mask);
    }

    /// <summary>
    /// Get the total count of clients in the system
    /// </summary>
    public int GetTotalClientsCount()
    {
        return ((ClientRepository)unitOfWork.Clients).GetTotalClientsCount();
    }
}