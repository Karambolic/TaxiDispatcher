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

        const string query = "INSERT INTO [Address] (streetName, streetNumber) VALUES (@name, @num); SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(query, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@name", address.StreetName);
        cmd.Parameters.AddWithValue("@num", address.StreetNumber);

        address.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Address address)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = "UPDATE [Address] SET streetName = @name, streetNumber = @num WHERE id = @id";

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

        using var cmd = new SqlCommand("SELECT * FROM [Address] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToAddress(reader) : null;
    }

    public List<Address> GetAll()
    {
        var list = new List<Address>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Address]", (SqlConnection)connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            list.Add(MapReaderToAddress(reader));

        return list;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("DELETE FROM [Address] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    private static Address MapReaderToAddress(SqlDataReader reader)
    {
        return new Address(
            (string)reader["streetName"],
            (string)reader["streetNumber"],
            (int)reader["id"]
        );
    }
}