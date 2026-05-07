using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class TransactionRepository(DbConnectionFactory connectionFactory) : IRepository<Transaction>
{
    public void Add(Transaction entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        const string sql = @"
            INSERT INTO Transactions (TransactionType, DriverId, ClientId, Amount, Comment, Timestamp)
            VALUES (@type, @drId, @clId, @amount, @comment, @time);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@type", (int)entity.TransactionType);
        cmd.Parameters.AddWithValue("@drId", entity.DriverId);
        cmd.Parameters.AddWithValue("@clId", entity.ClientId);
        cmd.Parameters.AddWithValue("@amount", entity.Amount);
        cmd.Parameters.AddWithValue("@comment", (object?)entity.Comment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@time", entity.Timestamp);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Transaction? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("SELECT * FROM Transactions WHERE Id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToTransaction(reader) : null;
    }

    public List<Transaction> GetAll()
    {
        var list = new List<Transaction>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var cmd = new SqlCommand("SELECT * FROM Transactions ORDER BY Timestamp DESC", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(MapReaderToTransaction(reader));
        }
        return list;
    }

    public bool Update(Transaction entity) => throw new NotSupportedException("Transactions cannot be edited.");

    public bool Delete(int id) => throw new NotSupportedException("Transactions cannot be deleted.");

    private Transaction MapReaderToTransaction(SqlDataReader reader)
    {
        return new Transaction
        {
            Id = (int)reader["Id"],
            TransactionType = (TransactionType)(int)reader["TransactionType"],
            DriverId = (int)reader["DriverId"],
            ClientId = (int)reader["ClientId"],
            Amount = (decimal)reader["Amount"],
            Comment = reader["Comment"] as string ?? string.Empty,
            Timestamp = (DateTime)reader["Timestamp"]
        };
    }
}