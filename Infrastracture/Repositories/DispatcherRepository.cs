using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class DispatcherRepository(DbConnectionFactory connectionFactory) : IDispatcherRepository
{
    public void Add(Dispatcher entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        const string sql = "INSERT INTO Dispatchers (FirstName, LastName, PhoneNumber) VALUES (@fn, @ln, @ph); SELECT SCOPE_IDENTITY();";
        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@fn", entity.FirstName);
        cmd.Parameters.AddWithValue("@ln", entity.LastName);
        cmd.Parameters.AddWithValue("@ph", entity.PhoneNumber);
        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Dispatcher? GetByPhone(string phone)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM Dispatchers WHERE PhoneNumber = @phone", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@phone", phone);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return new Dispatcher((string)reader["FirstName"], (string)reader["LastName"], (string)reader["PhoneNumber"], (int)reader["Id"]);

        return null;
    }

    public Dispatcher? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM Dispatchers WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return new Dispatcher((string)reader["FirstName"], (string)reader["LastName"], (string)reader["PhoneNumber"], (int)reader["Id"]);

        return null;
    }

    public bool Update(Dispatcher entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        const string sql = "UPDATE Dispatchers SET FirstName = @fn, LastName = @ln, PhoneNumber = @ph WHERE Id = @id";
        using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", entity.Id);
        cmd.Parameters.AddWithValue("@fn", entity.FirstName);
        cmd.Parameters.AddWithValue("@ln", entity.LastName);
        cmd.Parameters.AddWithValue("@ph", entity.PhoneNumber);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<Dispatcher> GetAll()
    {
        var list = new List<Dispatcher>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM Dispatchers", (SqlConnection)connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new Dispatcher((string)reader["FirstName"], (string)reader["LastName"], (string)reader["PhoneNumber"], (int)reader["Id"]));
        return list;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("DELETE FROM Dispatchers WHERE Id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    /// <summary>
    /// Retrieves the hashed password for a dispatcher based on their login
    /// </summary>
    /// <param name="login">The login of the dispatcher</param>
    /// <returns>The hashed password of the dispatcher if found; otherwise, null</returns>
    public string? GetHashedPasswordByLogin(string login)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT PasswordHashed FROM Credentials WHERE Login = @login", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@login", login);

        var result = cmd.ExecuteScalar();

        return result?.ToString();
    }

    public Dispatcher? GetByLogin(string login)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT d.* FROM Dispatchers d JOIN Credentials c ON d.Id = c.DispatcherId WHERE c.Login = @login", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@login", login);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return new Dispatcher((string)reader["FirstName"], (string)reader["LastName"], (string)reader["PhoneNumber"], (int)reader["Id"]);
        return null;
    }
}