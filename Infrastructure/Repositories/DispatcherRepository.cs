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
        const string sql = "INSERT INTO [Dispatcher] (firstName, lastName, phoneNumber) VALUES (@fn, @ln, @ph); SELECT SCOPE_IDENTITY();";
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
        using var cmd = new SqlCommand("SELECT * FROM [Dispatcher] WHERE phoneNumber = @phone", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@phone", phone);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return MapReaderToDispatcher(reader);

        return null;
    }

    public Dispatcher? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM [Dispatcher] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return MapReaderToDispatcher(reader);

        return null;
    }

    public bool Update(Dispatcher entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        const string sql = "UPDATE [Dispatcher] SET firstName = @fn, lastName = @ln, phoneNumber = @ph WHERE id = @id";
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
        using var cmd = new SqlCommand("SELECT * FROM [Dispatcher]", (SqlConnection)connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapReaderToDispatcher(reader));
        return list;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("DELETE FROM [Dispatcher] WHERE id = @id", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public string? GetHashedPasswordByLogin(string login)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT passwordHashed FROM [Credentials] WHERE login = @login", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@login", login);

        var result = cmd.ExecuteScalar();
        if (result == null || result == DBNull.Value) return null;

        return result.ToString()?.Trim();
    }

    public Dispatcher? GetByLogin(string login)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT d.* FROM [Dispatcher] d JOIN [Credentials] c ON d.id = c.dispatcherId WHERE c.login = @login", (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@login", login);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapReaderToDispatcher(reader);
        return null;
    }

    private static Dispatcher MapReaderToDispatcher(SqlDataReader reader)
    {
        return new Dispatcher(
            (string)reader["firstName"],
            (string)reader["lastName"],
            (string)reader["phoneNumber"],
            (int)reader["id"]
        );
    }
}