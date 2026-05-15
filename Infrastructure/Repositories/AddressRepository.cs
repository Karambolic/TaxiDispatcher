using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class AddressRepository(DbConnectionFactory connectionFactory) : IRepository<Address>
{
    public void Add(Address address)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        const string query = "INSERT INTO Addresss (StreetName, StreetNumber) VALUES (@name, @num); SELECT SCOPE_IDENTITY();";
        using var cmd = new SqlCommand(query, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@name", address.StreetName);
        cmd.Parameters.AddWithValue("@num", address.StreetNumber);
        address.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Address address)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        const string sql = "UPDATE Addresss SET StreetName = @name, StreetNumber = @num WHERE Id = @id";
        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", address.Id);
        cmd.Parameters.AddWithValue("@name", address.StreetName);
        cmd.Parameters.AddWithValue("@num", address.StreetNumber);
        return cmd.ExecuteNonQuery() > 0;
    }

    public Address? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM Addresss WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? new Address((string)reader["StreetName"], (string)reader["StreetNumber"], (int)reader["Id"]) : null;
    }

    public List<Address> GetAll()
    {
        var list = new List<Address>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM Addresss", (SqlConnection)connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new Address((string)reader["StreetName"], (string)reader["StreetNumber"], (int)reader["Id"]));
        return list;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("DELETE FROM Addresss WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }
}