using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class TransactionRepository(DbConnectionFactory connectionFactory) : IRepository<Transaction>
{
    public void Add(Transaction entity)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            INSERT INTO [Transaction] (typeId, driverId, clientId, amount, details, [timestamp])
            VALUES (@type, @drId, @clId, @amount, @details, @time);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@type", (int)entity.TransactionType);
        cmd.Parameters.AddWithValue("@drId", (object?)entity.DriverId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@clId", (object?)entity.ClientId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@amount", entity.Amount);
        cmd.Parameters.AddWithValue("@details", (object?)entity.Comment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@time", entity.Timestamp);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Transaction? GetById(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM [Transaction] WHERE [id] = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToTransaction(reader) : null;
    }

    public List<Transaction> GetAll()
    {
        var list = new List<Transaction>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM [Transaction] ORDER BY [timestamp] DESC", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(MapReaderToTransaction(reader));
        }
        return list;
    }

    public bool Update(Transaction entity) => throw new NotSupportedException("Transactions cannot be edited.");

    public bool Delete(int id) => throw new NotSupportedException("Transactions cannot be deleted.");

    static private Transaction MapReaderToTransaction(SqlDataReader reader)
    {
        return new Transaction
        {
            Id = (int)reader["id"],
            TransactionType = (TransactionType)(int)reader["typeId"],
            DriverId = reader["driverId"] != DBNull.Value ? (int)reader["driverId"] : null,
            ClientId = reader["clientId"] != DBNull.Value ? (int)reader["clientId"] : null,
            Amount = (decimal)reader["amount"],
            Comment = reader["details"]?.ToString() ?? string.Empty,
            Timestamp = (DateTime)reader["timestamp"]
        };
    }
}