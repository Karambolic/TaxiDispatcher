using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class ClientService(UnitOfWork unitOfWork)
{
    /// <summary>
    /// Get a client by their phone number. Returns null if not found
    /// </summary>
    public Client? GetClientByPhone(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
        return unitOfWork.Clients.GetByPhone(phoneNumber);
    }

    /// <summary>
    /// Creates a new client if one with the given phone number does not exist, or returns the existing client
    /// </summary>
    public Client GetOrCreateClient(string firstName, string lastName, string phoneNumber)
    {
        var existingClient = unitOfWork.Clients.GetByPhone(phoneNumber);

        if (existingClient != null)
        {
            return existingClient;
        }

        var newClient = new Client(firstName, lastName, phoneNumber);
        unitOfWork.Clients.Add(newClient);
        return newClient;
    }

    /// <summary>
    /// Get the order history for a specific client, ordered by creation date descending (from new to old)
    /// </summary>
    public List<Order> GetClientOrderHistory(int clientId)
    {
        return unitOfWork.Orders.GetAll()
            .Where(o => o.Client.Id == clientId)
            .OrderByDescending(o => o.CreatedAt).ToList();
    }

    /// <summary>
    /// Update the information of an existing client. Returns true if the update was successful, false otherwise
    /// </summary>
    public bool UpdateClientInfo(Client client)
    {
        return unitOfWork.Clients.Update(client);
    }

    /// <summary>
    /// Get all clients in the system. Returns a list of clients
    /// </summary>
    /// <returns></returns>
    public List<Client> GetAllClients() => unitOfWork.Clients.GetAll();
}