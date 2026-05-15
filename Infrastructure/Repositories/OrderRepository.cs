using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class OrderRepository(DbConnectionFactory connectionFactory) : IOrderRepository
{
    // SOLID: One source of truth for the SELECT query to avoid repeating syntax errors
    private const string BaseSelectQuery = @"
        SELECT o.*, 
               c.firstName as ClFN, c.lastName as ClLN, c.phoneNumber as ClPh,
               addrS.streetName as sStreet, addrS.streetNumber as sNum,
               addrE.streetName as eStreet, addrE.streetNumber as eNum,
               d.firstName as DrFN, d.lastName as DrLN, d.phoneNumber as DrPh, d.statusId as DrStatus,
               disp.firstName as DispName,
               t.name as TrfName
        FROM [Order] o
        INNER JOIN [Client] c ON o.clientId = c.id
        INNER JOIN [Address] addrS ON o.addressStartId = addrS.id
        INNER JOIN [Address] addrE ON o.addressEndId = addrE.id
        INNER JOIN [Dispatcher] disp ON o.dispatcherId = disp.id
        INNER JOIN [Tariff] t ON o.tariffId = t.id
        LEFT JOIN [Driver] d ON o.driverId = d.id";

    public List<Order> GetActiveOrders()
    {
        var list = new List<Order>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        string sql = $"{BaseSelectQuery} WHERE o.statusId IN (1, 2) ORDER BY o.createdAt DESC";
        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(MapReaderToOrder(reader));
        return list;
    }

    public Order? GetById(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        // FIXED: Used BaseSelectQuery instead of manual string with 'aS'
        string sql = $"{BaseSelectQuery} WHERE o.id = @id";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToOrder(reader) : null;
    }

    public List<Order> GetAll()
    {
        var list = new List<Order>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        // FIXED: Used BaseSelectQuery
        string sql = $"{BaseSelectQuery} ORDER BY o.createdAt DESC";
        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(MapReaderToOrder(reader));
        return list;
    }

    public List<Order> GetOrdersByClientId(int clientId)
    {
        var list = new List<Order>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        // FIXED: Used BaseSelectQuery
        string sql = $"{BaseSelectQuery} WHERE o.clientId = @clientId";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@clientId", clientId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(MapReaderToOrder(reader));
        return list;
    }

    private static Order MapReaderToOrder(SqlDataReader reader)
    {
        return new Order
        {
            Id = (int)reader["id"],
            Client = new Client((string)reader["ClFN"], (string)reader["ClLN"], (string)reader["ClPh"], (int)reader["clientId"]),
            AddressStart = new Address((string)reader["sStreet"], (string)reader["sNum"], (int)reader["addressStartId"]),
            AddressEnd = new Address((string)reader["eStreet"], (string)reader["eNum"], (int)reader["addressEndId"]),
            Dispatcher = new Dispatcher { Id = (int)reader["dispatcherId"], FirstName = reader["DispName"].ToString()! },
            Tariff = new Tariff { Id = (int)reader["tariffId"], Name = reader["TrfName"].ToString()! },
            Driver = reader["driverId"] == DBNull.Value ? null : new Driver(
                (string)reader["DrFN"], (string)reader["DrLN"], (string)reader["DrPh"],
                (int)reader["driverId"], (DriverStatus)(int)reader["DrStatus"]),
            Status = (OrderStatus)(int)reader["statusId"],
            CreatedAt = (DateTime)reader["createdAt"],
            PassengerCount = (int)reader["passengersCount"],
            Comment = reader["comment"]?.ToString() ?? string.Empty,
            FinalDistanceKm = reader["finalDistanceKm"] != DBNull.Value ? Convert.ToSingle(reader["finalDistanceKm"]) : 0f,
            FinalPrice = reader["finalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["finalPrice"]) : 0m,
            StartedAt = reader["startedAt"] as DateTime?,
            FinishedAt = reader["finishedAt"] as DateTime?
        };
    }

    public void Add(Order order)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        const string sql = @"
            INSERT INTO [Order] (clientId, dispatcherId, driverId, tariffId, addressStartId, addressEndId, statusId, createdAt, passengersCount, finalDistanceKm, finalPrice, comment)
            VALUES (@clId, @dispId, @drId, @tId, @addrS, @addrE, @status, @created, @pass, @dist, @price, @cmt);
            SELECT SCOPE_IDENTITY();";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@clId", order.Client.Id);
        cmd.Parameters.AddWithValue("@dispId", order.Dispatcher.Id);
        cmd.Parameters.AddWithValue("@drId", (object?)order.Driver?.Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tId", order.Tariff.Id);
        cmd.Parameters.AddWithValue("@addrS", order.AddressStart.Id);
        cmd.Parameters.AddWithValue("@addrE", order.AddressEnd.Id);
        cmd.Parameters.AddWithValue("@status", (int)order.Status);
        cmd.Parameters.AddWithValue("@created", order.CreatedAt);
        cmd.Parameters.AddWithValue("@pass", order.PassengerCount);
        cmd.Parameters.AddWithValue("@dist", order.FinalDistanceKm);
        cmd.Parameters.AddWithValue("@price", order.FinalPrice);
        cmd.Parameters.AddWithValue("@cmt", (object?)order.Comment ?? DBNull.Value);
        order.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Order entity)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        const string sql = @"
            UPDATE [Order] SET driverId = @drId, statusId = @status, finalDistanceKm = @dist, 
            finalPrice = @price, finishedAt = @finished, startedAt = @started WHERE id = @id";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", entity.Id);
        cmd.Parameters.AddWithValue("@drId", (object?)entity.Driver?.Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", (int)entity.Status);
        cmd.Parameters.AddWithValue("@dist", entity.FinalDistanceKm);
        cmd.Parameters.AddWithValue("@price", entity.FinalPrice);
        cmd.Parameters.AddWithValue("@finished", (object?)entity.FinishedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@started", (object?)entity.StartedAt ?? DBNull.Value);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("DELETE FROM [Order] WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<OrderPeriodReport> GetOrdersByPeriod(DateTime start, DateTime end)
    {
        var list = new List<OrderPeriodReport>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();
        const string sql = "SELECT id, createdAt, finalPrice FROM [Order] WHERE createdAt BETWEEN @start AND @end";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@start", start);
        cmd.Parameters.AddWithValue("@end", end);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new OrderPeriodReport
            {
                Id = (int)reader["id"],
                CreatedAt = (DateTime)reader["createdAt"],
                FinalPrice = reader["finalPrice"] != DBNull.Value ? (decimal)reader["finalPrice"] : 0
            });
        }
        return list;
    }
}