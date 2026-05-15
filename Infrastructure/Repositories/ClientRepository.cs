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
            INSERT INTO [Client] (firstName, lastName, phoneNumber) 
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

        using var cmd = new SqlCommand("SELECT * FROM [Client] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToClient(reader) : null;
    }

    public Client? GetByPhone(string phone)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Client] WHERE phoneNumber = @phone", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@phone", phone);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToClient(reader) : null;
    }

    public List<Client> GetAll()
    {
        var clients = new List<Client>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Client]", (SqlConnection)connection);
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
            UPDATE [Client] 
            SET firstName = @fn, lastName = @ln, phoneNumber = @ph 
            WHERE id = @id";

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

        using var cmd = new SqlCommand("DELETE FROM [Client] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Query 6.2 - Find Client by phone mask
    public List<Client> GetClientByPhoneMask(string mask)
    {
        var list = new List<Client>();
        using var conn = connectionFactory.CreateConnection();
        conn.Open();

        using var cmd = new SqlCommand("SELECT id, firstName, lastName, phoneNumber FROM [Client] WHERE phoneNumber LIKE @mask", (SqlConnection)conn);
        cmd.Parameters.AddWithValue("@mask", mask + "%");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToClient(reader));
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

    static private Client MapReaderToClient(SqlDataReader reader)
    {
        return new Client(
            (string)reader["firstName"],
            (string)reader["lastName"],
            (string)reader["phoneNumber"],
            (int)reader["id"]
        );
    }
}