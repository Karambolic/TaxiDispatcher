using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class TariffRepository(DbConnectionFactory connectionFactory) : IRepository<Tariff>
{
    public void Add(Tariff entity)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            INSERT INTO [Tariff] ([name], [pricePerKm]) 
            VALUES (@name, @price); 
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", entity.Name);
        cmd.Parameters.AddWithValue("@price", entity.PricePerKm);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Tariff? GetById(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Tariff] WHERE [id] = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToTariff(reader) : null;
    }

    public List<Tariff> GetAll()
    {
        var list = new List<Tariff>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Tariff]", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(MapReaderToTariff(reader));
        }
        return list;
    }

    public bool Update(Tariff entity)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = "UPDATE [Tariff] SET [name] = @name, [pricePerKm] = @price WHERE [id] = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", entity.Name);
        cmd.Parameters.AddWithValue("@price", entity.PricePerKm);
        cmd.Parameters.AddWithValue("@id", entity.Id);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("DELETE FROM [Tariff] WHERE [id] = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    private Tariff MapReaderToTariff(SqlDataReader reader)
    {
        return new Tariff
        {
            Id = (int)reader["id"],
            Name = reader["name"].ToString() ?? string.Empty,
            PricePerKm = Convert.ToDecimal(reader["pricePerKm"])
        };
    }
}