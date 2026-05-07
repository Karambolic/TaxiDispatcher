using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class AutomobileRepository(DbConnectionFactory connectionFactory) : IRepository<Automobile>
{
    public void Add(Automobile entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sqlAuto = @"
            INSERT INTO Automobiles (PlateNumber, Make, Model, [Year], Capacity)
            VALUES (@plate, @make, @model, @year, @capacity);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sqlAuto, connection);
        cmd.Parameters.AddWithValue("@plate", entity.PlateNumber);
        cmd.Parameters.AddWithValue("@make", entity.Make);
        cmd.Parameters.AddWithValue("@model", entity.Model);
        cmd.Parameters.AddWithValue("@year", entity.Year);
        cmd.Parameters.AddWithValue("@capacity", entity.Capacity);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());

        UpdateAvailableTariffs(entity, connection);
    }

    public Automobile? GetById(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = "SELECT * FROM Automobiles WHERE Id = @id";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var automobile = MapReaderToAutomobile(reader);
        reader.Close(); // As reader will be used again for tariffs in the inner scope, have to close it

        automobile.AllowedTariffs = LoadAvailableTariffs(automobile.Id, connection);

        return automobile;
    }

    public List<Automobile> GetAll()
    {
        var result = new List<Automobile>();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = "SELECT * FROM Automobiles";
        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            result.Add(MapReaderToAutomobile(reader));

        reader.Close();

        foreach (var auto in result)
        {
            auto.AllowedTariffs = LoadAvailableTariffs(auto.Id, connection);
        }

        return result;
    }

    public bool Update(Automobile entity)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            UPDATE Automobiles 
            SET PlateNumber = @plate, Make = @make, Model = @model, [Year] = @year, Capacity = @capacity
            WHERE Id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", entity.Id);
        cmd.Parameters.AddWithValue("@plate", entity.PlateNumber);
        cmd.Parameters.AddWithValue("@make", entity.Make);
        cmd.Parameters.AddWithValue("@model", entity.Model);
        cmd.Parameters.AddWithValue("@year", entity.Year);
        cmd.Parameters.AddWithValue("@capacity", entity.Capacity);

        bool updated = cmd.ExecuteNonQuery() > 0;

        if (updated)
        {
            UpdateAvailableTariffs(entity, connection);
        }

        return updated;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        // Delete related entries in TariffAvailability first to maintain referential integrity
        const string sqlDelRel = "DELETE FROM TariffAvailability WHERE automobileId = @id";
        using var cmdRel = new SqlCommand(sqlDelRel, connection);
        cmdRel.Parameters.AddWithValue("@id", id);
        cmdRel.ExecuteNonQuery();

        const string sqlAuto = "DELETE FROM Automobiles WHERE Id = @id";
        using var cmdAuto = new SqlCommand(sqlAuto, connection);
        cmdAuto.Parameters.AddWithValue("@id", id);

        return cmdAuto.ExecuteNonQuery() > 0;
    }

    // Aux methods for tariffs handling 

    private List<Tariff> LoadAvailableTariffs(int automobileId, SqlConnection connection)
    {
        var tariffs = new List<Tariff>();
        const string sql = @"
            SELECT t.* FROM Tariffs t
            JOIN TariffAvailability ta ON t.Id = ta.tariffId
            WHERE ta.automobileId = @aId";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@aId", automobileId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tariffs.Add(new Tariff(
                (string)reader["Name"],
                (decimal)reader["PricePerKm"],
                (int)reader["Id"]));
        }
        return tariffs;
    }

    private void UpdateAvailableTariffs(Automobile entity, SqlConnection connection)
    {
        // Delete existing relationships
        const string sqlDelete = "DELETE FROM TariffAvailability WHERE automobileId = @aId";
        using var cmdDel = new SqlCommand(sqlDelete, connection);
        cmdDel.Parameters.AddWithValue("@aId", entity.Id);
        cmdDel.ExecuteNonQuery();

        // Add new relationships
        if (entity.AllowedTariffs != null && entity.AllowedTariffs.Count > 0)
        {
            foreach (var tariff in entity.AllowedTariffs)
            {
                const string sqlInsert = "INSERT INTO TariffAvailability (automobileId, tariffId) VALUES (@aId, @tId)";
                using var cmdIns = new SqlCommand(sqlInsert, connection);
                cmdIns.Parameters.AddWithValue("@aId", entity.Id);
                cmdIns.Parameters.AddWithValue("@tId", tariff.Id);
                cmdIns.ExecuteNonQuery();
            }
        }
    }

    private Automobile MapReaderToAutomobile(SqlDataReader reader)
    {
        return new Automobile(
            (string)reader["PlateNumber"],
            (string)reader["Make"],
            (string)reader["Model"],
            (int)reader["Year"],
            (int)reader["Capacity"],
            (int)reader["Id"]);
    }
}