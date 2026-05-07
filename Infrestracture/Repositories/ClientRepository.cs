using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class ClientRepository(DbConnectionFactory connectionFactory) : IClientRepository
{
    public void Add(Client client)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            INSERT INTO Clients (FirstName, LastName, PhoneNumber) 
            VALUES (@fn, @ln, @ph); 
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@fn", client.FirstName);
        cmd.Parameters.AddWithValue("@ln", client.LastName);
        cmd.Parameters.AddWithValue("@ph", client.PhoneNumber);

        client.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Client? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Clients WHERE Id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToClient(reader) : null;
    }

    public Client? GetByPhone(string phone)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Clients WHERE PhoneNumber = @phone", connection);
        cmd.Parameters.AddWithValue("@phone", phone);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToClient(reader) : null;
    }

    public List<Client> GetAll()
    {
        var clients = new List<Client>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Clients", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            clients.Add(MapReaderToClient(reader));
        }

        return clients;
    }

    public bool Update(Client entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            UPDATE Clients 
            SET FirstName = @fn, LastName = @ln, PhoneNumber = @ph 
            WHERE Id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", entity.Id);
        cmd.Parameters.AddWithValue("@fn", entity.FirstName);
        cmd.Parameters.AddWithValue("@ln", entity.LastName);
        cmd.Parameters.AddWithValue("@ph", entity.PhoneNumber);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("DELETE FROM Clients WHERE Id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // To avoid code duplication
    private Client MapReaderToClient(SqlDataReader reader)
    {
        return new Client(
            (string)reader["FirstName"],
            (string)reader["LastName"],
            (string)reader["PhoneNumber"],
            (int)reader["Id"]
        );
    }
}