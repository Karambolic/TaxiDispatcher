using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class OrderRepository(DbConnectionFactory connectionFactory) : IOrderRepository
{
    public void Add(Order order)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            INSERT INTO Orders 
            (ClientId, DispatcherId, DriverId, AddressStartId, AddressEndId, Status, CreatedAt, PassengerCount, FinalDistanceKm, FinalPrice, Comment)
            VALUES 
            (@clId, @dispId, @drId, @addrS, @addrE, @status, @created, @pass, @dist, @price, @cmt);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@clId", order.Client.Id);
        cmd.Parameters.AddWithValue("@dispId", order.Dispatcher.Id);
        cmd.Parameters.AddWithValue("@drId", (object?)order.Driver?.Id ?? DBNull.Value);
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

    public Order? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            SELECT o.*, 
                 c.FirstName as ClFN, c.LastName as ClLN, c.PhoneNumber as ClPh,
                 d.FirstName as DrFN, d.LastName as DrLN, d.PhoneNumber as DrPh, d.Status as DrStatus
            FROM Orders o
            JOIN Clients c ON o.ClientId = c.Id
            LEFT JOIN Drivers d ON o.DriverId = d.Id
            WHERE o.Id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReaderToOrder(reader) : null;
    }

    public List<Order> GetAll()
    {
        var list = new List<Order>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            SELECT o.*, 
                   c.FirstName as ClFN, c.LastName as ClLN, c.PhoneNumber as ClPh,
                   d.FirstName as DrFN, d.LastName as DrLN, d.PhoneNumber as DrPh, d.Status as DrStatus
            FROM Orders o
            JOIN Clients c ON o.ClientId = c.Id
            LEFT JOIN Drivers d ON o.DriverId = d.Id
            ORDER BY o.CreatedAt DESC";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToOrder(reader));
        }
        return list;
    }

    public bool Update(Order entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            UPDATE Orders SET 
                DriverId = @drId, 
                Status = @status, 
                FinalDistanceKm = @dist, 
                FinalPrice = @price, 
                FinishedAt = @finished
            WHERE Id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", entity.Id);
        cmd.Parameters.AddWithValue("@drId", (object?)entity.Driver?.Id ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", (int)entity.Status);
        cmd.Parameters.AddWithValue("@dist", entity.FinalDistanceKm);
        cmd.Parameters.AddWithValue("@price", entity.FinalPrice);
        cmd.Parameters.AddWithValue("@finished", (object?)entity.FinishedAt ?? DBNull.Value);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = new SqlCommand("DELETE FROM Orders WHERE Id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    private Order MapReaderToOrder(SqlDataReader reader)
    {
        var client = new Client((string)reader["ClFN"], (string)reader["ClLN"], (string)reader["ClPh"], (int)reader["ClientId"]);

        var dispatcher = new Dispatcher { Id = (int)reader["DispatcherId"] };

        var tariff = new Tariff { Id = (int)reader["TariffId"] };

        var addrStart = new Address { Id = (int)reader["AddressStartId"] };
        var addrEnd = new Address { Id = (int)reader["AddressEndId"] };

        Driver? driver = null;
        if (reader["DriverId"] != DBNull.Value)
        {
            driver = new Driver(
                (string)reader["DrFN"],
                (string)reader["DrLN"],
                (string)reader["DrPh"],
                (int)reader["DriverId"],
                (DriverStatus)(int)reader["DrStatus"]
            );
        }

        return new Order
        {
            Id = (int)reader["Id"],
            Client = client,
            Dispatcher = dispatcher,
            Driver = driver,
            Tariff = tariff,
            AddressStart = addrStart,
            AddressEnd = addrEnd,
            Status = (OrderStatus)(int)reader["Status"],
            CreatedAt = (DateTime)reader["CreatedAt"],
            StartedAt = reader["StartedAt"] as DateTime?,
            FinishedAt = reader["FinishedAt"] as DateTime?,
            PassengerCount = (int)reader["PassengerCount"],
            FinalDistanceKm = Convert.ToSingle(reader["FinalDistanceKm"]),
            FinalPrice = Convert.ToDecimal(reader["FinalPrice"]),
            Comment = reader["Comment"]?.ToString() ?? string.Empty
        };
    }
}
}