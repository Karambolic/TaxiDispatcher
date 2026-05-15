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
            INSERT INTO Client (FirstName, LastName, PhoneNumber) 
            VALUES (@fn, @ln, @ph); 
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@fn", client.FirstName);
        cmd.Parameters.AddWithValue("@ln", client.LastName);
        cmd.Parameters.AddWithValue("@ph", client.PhoneNumber);

        client.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Client? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Client WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToClient(reader) : null;
    }

    public Client? GetByPhone(string phone)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Client WHERE PhoneNumber = @phone", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@phone", phone);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToClient(reader) : null;
    }

    public List<Client> GetAll()
    {
        var Client = new List<Client>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Client", (SqlConnection)connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Client.Add(MapReaderToClient(reader));
        }

        return Client;
    }

    public bool Update(Client entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            UPDATE Client 
            SET FirstName = @fn, LastName = @ln, PhoneNumber = @ph 
            WHERE Id = @id";

        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
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

        using var cmd = new SqlCommand("DELETE FROM Client WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // To avoid code duplication
    static private Client MapReaderToClient(SqlDataReader reader)
    {
        return new Client(
            (string)reader["FirstName"],
            (string)reader["LastName"],
            (string)reader["PhoneNumber"],
            (int)reader["Id"]
        );
    }

    // Query 6.2 - Find Client by phone mask
    public List<Client> GetClientByPhoneMask(string mask)
    {
        var list = new List<Client>();
        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        // Using @mask parameter for safety
        using var cmd = new SqlCommand("SELECT firstName, lastName, phoneNumber, id FROM [Client] WHERE phoneNumber LIKE @mask", (SqlConnection)conn);
        cmd.Parameters.AddWithValue("@mask", mask + "%");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Client((string)reader["FirstName"], (string)reader["LastName"], (string)reader["PhoneNumber"], (int)reader["Id"]));
        }
        return list;
    }

    // Query 6.4 - Total count of Client
    public int GetTotalClientCount()
    {
        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM [Client]", (SqlConnection)conn);
        
        return (int)cmd.ExecuteScalar();
    }
}