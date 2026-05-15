using Microsoft.Data.SqlClient;
using Domain.DTO;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

public class ReportRepository(DbConnectionFactory factory) : IReportRepository
{
    // Query 6.1 - List of cars and their drivers
    public List<AutoDriverReport> GetFleetReport()
    {
        var list = new List<AutoDriverReport>();
        using var conn = (SqlConnection)factory.CreateConnection();
        conn.Open();

        string sql = @"SELECT a.make, a.model, a.[year], d.lastName 
                       FROM [Automobile] a 
                       JOIN [Driver] d ON a.driverId = d.id 
                       ORDER BY a.[year] ASC";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new AutoDriverReport
            {
                Make = reader["make"].ToString() ?? "",
                Model = reader["model"].ToString() ?? "",
                Year = (int)reader["year"],
                DriverLastName = reader["lastName"].ToString() ?? ""
            });
        }
        return list;    
    }

    // Query 6.5 - Average price per tariff
    public List<TariffAvgPriceReport> GetTariffStatus()
    {
        var list = new List<TariffAvgPriceReport>();
        using var conn = (SqlConnection)factory.CreateConnection();
        conn.Open();

        string sql = @"SELECT t.name, AVG(o.finalPrice) as Average 
                       FROM [Tariff] t 
                       JOIN [Order] o ON t.id = o.tariffId 
                       GROUP BY t.id, t.name";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new TariffAvgPriceReport
            {
                Name = reader["name"].ToString() ?? "",
                AvgPrice = Convert.ToDecimal(reader["Average"])
            });
        }
        return list;
    }

    // Query 6.6 - The most active dispatcher
    public string? GetTopDispatcher()
    {
        using var conn = (SqlConnection)factory.CreateConnection();
        conn.Open();

        string sql = @"SELECT dispatcherId, COUNT(id) AS Total 
                       FROM [Order] GROUP BY dispatcherId 
                       HAVING COUNT(id) >= ALL (SELECT COUNT(id) FROM [Order] GROUP BY dispatcherId)";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return new string("Dispatcher  with id: " + reader["dispatcherId"] + " with " + reader["total"] + " orders");
       
        return null;
    }

    // Query 6.7 - Max order for every client
    public List<ClientMaxOrderReport> GetMaxOrdersPerClient()
    {
        var list = new List<ClientMaxOrderReport>();
        using var conn = (SqlConnection)factory.CreateConnection();
        conn.Open();
        string sql = @"SELECT o1.clientId, o1.id, o1.finalPrice 
                       FROM [Order] o1 
                       WHERE o1.finalPrice = (SELECT MAX(o2.finalPrice) 
                                              FROM [Order] o2 
                                              WHERE o2.clientId = o1.clientId)";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ClientMaxOrderReport
            {
                ClientId = (int)reader["clientId"],
                OrderId = (int)reader["id"],
                FinalPrice = (decimal)reader["finalPrice"]
            });
        }
        return list;
    }

    // Query 6.8 - Drivers who haven't done any orders
    public List<IdleDriverReport> GetIdleDrivers()
    {
        var list = new List<IdleDriverReport>();
        using var conn = (SqlConnection)factory.CreateConnection();
        conn.Open();
        string sql = "SELECT d.id, d.lastName FROM [Driver] d LEFT JOIN [Order] o ON d.id = o.driverId WHERE o.id IS NULL";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new IdleDriverReport { Id = (int)reader["id"], LastName = (string)reader["lastName"] });
        }
        return list;
    }

    // Query 6.9 - Status of tariff usage
    public List<TariffStatusReport> GetTariffUsageStatus()
    {
        var list = new List<TariffStatusReport>();
        using var conn = (SqlConnection)factory.CreateConnection();
        conn.Open();
        string sql = @"SELECT name, 'Used' as Status FROM [Tariff] WHERE id IN (SELECT tariffId FROM [Order])
                   UNION
                   SELECT name, 'Not Used' FROM [Tariff] WHERE id NOT IN (SELECT tariffId FROM [Order])";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new TariffStatusReport { Name = (string)reader["name"], Status = (string)reader["Status"] });
        }
        return list;
    }
}
