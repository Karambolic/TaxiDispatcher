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
            INSERT INTO Drivers (FirstName, LastName, PhoneNumber, Status)
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

        using var cmd = new SqlCommand("SELECT * FROM Drivers WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToDriver(reader) : null;
    }

    public List<Driver> GetByStatus(DriverStatus status)
    {
        var list = new List<Driver>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Drivers WHERE Status = @status", (SqlConnection)connection);
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

        using var cmd = new SqlCommand("SELECT * FROM Drivers", (SqlConnection)connection);
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
            UPDATE Drivers 
            SET FirstName = @fn, 
                LastName = @ln, 
                PhoneNumber = @ph, 
                Status = @status 
            WHERE Id = @id";

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

        using var cmd = new SqlCommand("DELETE FROM Drivers WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    private Driver MapReaderToDriver(SqlDataReader reader)
    {
        return new Driver(
            (string)reader["FirstName"],
            (string)reader["LastName"],
            (string)reader["PhoneNumber"],
            (int)reader["Id"],
            (DriverStatus)(int)reader["Status"]
        );
    }
}