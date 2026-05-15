using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class AutomobileRepository(DbConnectionFactory connectionFactory) : IRepository<Automobile>
{
    public void Add(Automobile entity)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sqlAuto = @"
            INSERT INTO [Automobile] (plateNumber, make, model, [year], capacity, driverId)
            VALUES (@plate, @make, @model, @year, @capacity, @driverId);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sqlAuto, connection);
        cmd.Parameters.AddWithValue("@plate", entity.PlateNumber);
        cmd.Parameters.AddWithValue("@make", entity.Make);
        cmd.Parameters.AddWithValue("@model", entity.Model);
        cmd.Parameters.AddWithValue("@year", entity.Year);
        cmd.Parameters.AddWithValue("@capacity", entity.Capacity);
        cmd.Parameters.AddWithValue("@driverId", (object?)entity.Driver?.Id ?? DBNull.Value);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());

        UpdateAvailableTariffs(entity, connection);
    }

    public Automobile? GetById(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            SELECT a.*, d.firstName, d.lastName, d.phoneNumber, d.statusId as driverStatus
            FROM [Automobile] a
            LEFT JOIN [Driver] d ON a.driverId = d.id
            WHERE a.id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var automobile = MapReaderToAutomobile(reader);
        reader.Close();

        automobile.AllowedTariffs = LoadAvailableTariffs(automobile.Id, connection);

        return automobile;
    }

    public List<Automobile> GetAll()
    {
        var result = new List<Automobile>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            SELECT a.*, d.firstName, d.lastName, d.phoneNumber, d.statusId as driverStatus
            FROM [Automobile] a
            LEFT JOIN [Driver] d ON a.driverId = d.id";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            result.Add(MapReaderToAutomobile(reader));

        reader.Close();

        foreach (var auto in result)
            auto.AllowedTariffs = LoadAvailableTariffs(auto.Id, connection);

        return result;
    }

    public bool Update(Automobile entity)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            UPDATE [Automobile] 
            SET plateNumber = @plate, make = @make, model = @model, 
                [year] = @year, capacity = @capacity, driverId = @driverId
            WHERE id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", entity.Id);
        cmd.Parameters.AddWithValue("@plate", entity.PlateNumber);
        cmd.Parameters.AddWithValue("@make", entity.Make);
        cmd.Parameters.AddWithValue("@model", entity.Model);
        cmd.Parameters.AddWithValue("@year", entity.Year);
        cmd.Parameters.AddWithValue("@capacity", entity.Capacity);
        cmd.Parameters.AddWithValue("@driverId", (object?)entity.Driver?.Id ?? DBNull.Value);

        bool updated = cmd.ExecuteNonQuery() > 0;

        if (updated)
        {
            UpdateAvailableTariffs(entity, connection);
        }

        return updated;
    }

    public bool Delete(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sqlDelRel = "DELETE FROM [TariffAvailability] WHERE automobileId = @id";
        using var cmdRel = new SqlCommand(sqlDelRel, connection);
        cmdRel.Parameters.AddWithValue("@id", id);
        cmdRel.ExecuteNonQuery();

        const string sqlAuto = "DELETE FROM [Automobile] WHERE id = @id";
        using var cmdAuto = new SqlCommand(sqlAuto, connection);
        cmdAuto.Parameters.AddWithValue("@id", id);

        return cmdAuto.ExecuteNonQuery() > 0;
    }

    static private Automobile MapReaderToAutomobile(SqlDataReader reader)
    {
        Driver? linkedDriver = null;

        if (reader["driverId"] != DBNull.Value)
        {
            linkedDriver = new Driver(
                (string)reader["firstName"],
                (string)reader["lastName"],
                (string)reader["phoneNumber"],
                (int)reader["driverId"],
                (DriverStatus)(int)reader["driverStatus"]
            );
        }

        return new Automobile(
            (string)reader["plateNumber"],
            (string)reader["make"],
            (string)reader["model"],
            (int)reader["year"],
            (int)reader["capacity"],
            linkedDriver!,
            (int)reader["id"]
        );
    }

    static private List<Tariff> LoadAvailableTariffs(int automobileId, SqlConnection connection)
    {
        var tariffs = new List<Tariff>();
        const string sql = @"
            SELECT t.* FROM [Tariff] t
            JOIN [TariffAvailability] ta ON t.id = ta.tariffId
            WHERE ta.automobileId = @aId";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@aId", automobileId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tariffs.Add(new Tariff
            {
                Id = (int)reader["id"],
                Name = (string)reader["name"],
                PricePerKm = (decimal)reader["pricePerKm"]
            });
        }
        return tariffs;
    }

    static private void UpdateAvailableTariffs(Automobile entity, SqlConnection connection)
    {
        const string sqlDelete = "DELETE FROM [TariffAvailability] WHERE automobileId = @aId";
        using var cmdDel = new SqlCommand(sqlDelete, connection);
        cmdDel.Parameters.AddWithValue("@aId", entity.Id);
        cmdDel.ExecuteNonQuery();

        if (entity.AllowedTariffs != null && entity.AllowedTariffs.Count > 0)
        {
            foreach (var tariff in entity.AllowedTariffs)
            {
                const string sqlInsert = "INSERT INTO [TariffAvailability] (automobileId, tariffId) VALUES (@aId, @tId)";
                using var cmdIns = new SqlCommand(sqlInsert, connection);
                cmdIns.Parameters.AddWithValue("@aId", entity.Id);
                cmdIns.Parameters.AddWithValue("@tId", tariff.Id);
                cmdIns.ExecuteNonQuery();
            }
        }
    }
}