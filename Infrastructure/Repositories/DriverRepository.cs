using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class DriverRepository(DbConnectionFactory connectionFactory) : IDriverRepository
{
    public void Add(Driver entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            INSERT INTO [Driver] (firstName, lastName, phoneNumber, statusId)
            VALUES (@fn, @ln, @ph, @status);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@fn", entity.FirstName);
        cmd.Parameters.AddWithValue("@ln", entity.LastName);
        cmd.Parameters.AddWithValue("@ph", entity.PhoneNumber);
        cmd.Parameters.AddWithValue("@status", (int)entity.Status);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Driver? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Driver] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToDriver(reader) : null;
    }

    public List<Driver> GetByStatus(DriverStatus status)
    {
        var list = new List<Driver>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Driver] WHERE statusId = @status", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@status", (int)status);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToDriver(reader));
        }
        return list;
    }

    public List<Driver> GetAll()
    {
        var list = new List<Driver>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Driver]", (SqlConnection)connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToDriver(reader));
        }
        return list;
    }

    public bool Update(Driver entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            UPDATE [Driver] 
            SET firstName = @fn, 
                lastName = @ln, 
                phoneNumber = @ph, 
                statusId = @status 
            WHERE id = @id";

        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@fn", entity.FirstName);
        cmd.Parameters.AddWithValue("@ln", entity.LastName);
        cmd.Parameters.AddWithValue("@ph", entity.PhoneNumber);
        cmd.Parameters.AddWithValue("@status", (int)entity.Status);
        cmd.Parameters.AddWithValue("@id", entity.Id);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("DELETE FROM [Driver] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    static private Driver MapReaderToDriver(SqlDataReader reader)
    {
        return new Driver(
            (string)reader["firstName"],
            (string)reader["lastName"],
            (string)reader["phoneNumber"],
            (int)reader["id"],
            (DriverStatus)(int)reader["statusId"]
        );
    }
}