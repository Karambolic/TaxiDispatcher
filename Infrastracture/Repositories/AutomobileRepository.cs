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
            INSERT INTO Automobiles (PlateNumber, Make, Model, [Year], Capacity, DriverId)
            VALUES (@plate, @make, @model, @year, @capacity, @driverId);
            SELECT SCOPE_IDENTITY();";

        using var cmd = new SqlCommand(sqlAuto, connection);
        cmd.Parameters.AddWithValue("@plate", entity.PlateNumber);
        cmd.Parameters.AddWithValue("@make", entity.Make);
        cmd.Parameters.AddWithValue("@model", entity.Model);
        cmd.Parameters.AddWithValue("@year", entity.Year);
        cmd.Parameters.AddWithValue("@capacity", entity.Capacity);
        // Handle null driver
        cmd.Parameters.AddWithValue("@driverId", (object?)entity.Driver?.Id ?? DBNull.Value);

        entity.Id = Convert.ToInt32(cmd.ExecuteScalar());

        UpdateAvailableTariffs(entity, connection);
    }

    public Automobile? GetById(int id)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        // Left join to get driver details if they exist
        const string sql = @"
            SELECT a.*, d.FirstName, d.LastName, d.PhoneNumber, d.Status as DriverStatus
            FROM Automobiles a
            LEFT JOIN Drivers d ON a.DriverId = d.Id
            WHERE a.Id = @id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var automobile = MapReaderToAutomobile(reader);
        reader.Close();

        automobile.AllowedTariffs = LoadAvailableTariffs(automobile.Id, connection);

        return automobile;
    }

    /// <summary>
    /// Total taxi fleet report with driver details for each automobile
    /// </summary>
    /// <returns>List of automobiles with their associated drivers</returns>
    public List<Automobile> GetAll()
    {
        var result = new List<Automobile>();
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        connection.Open();

        const string sql = @"
            SELECT a.*, d.FirstName, d.LastName, d.PhoneNumber, d.Status as DriverStatus
            FROM Automobiles a
            LEFT JOIN Drivers d ON a.DriverId = d.Id";

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
            UPDATE Automobiles 
            SET PlateNumber = @plate, Make = @make, Model = @model, 
                [Year] = @year, Capacity = @capacity, DriverId = @driverId
            WHERE Id = @id";

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

        const string sqlDelRel = "DELETE FROM TariffAvailability WHERE automobileId = @id";
        using var cmdRel = new SqlCommand(sqlDelRel, connection);
        cmdRel.Parameters.AddWithValue("@id", id);
        cmdRel.ExecuteNonQuery();

        const string sqlAuto = "DELETE FROM Automobiles WHERE Id = @id";
        using var cmdAuto = new SqlCommand(sqlAuto, connection);
        cmdAuto.Parameters.AddWithValue("@id", id);

        return cmdAuto.ExecuteNonQuery() > 0;
    }

    static private Automobile MapReaderToAutomobile(SqlDataReader reader)
    {
        // Determine if there is a driver linked in this row
        Driver? linkedDriver = null;

        if (reader["DriverId"] != DBNull.Value)
        {
            linkedDriver = new Driver
            {
                Id = (int)reader["DriverId"],
                FirstName = (string)reader["FirstName"],
                LastName = (string)reader["LastName"],
                PhoneNumber = (string)reader["PhoneNumber"],
                Status = (DriverStatus)(int)reader["DriverStatus"]
            };
        }

        // Create the automobile using the constructor, passing the driver.
        // The 'linkedDriver' will be null if no DriverId was found in the DB
        return new Automobile(
            (string)reader["PlateNumber"],
            (string)reader["Make"],
            (string)reader["Model"],
            (int)reader["Year"],
            (int)reader["Capacity"],
            linkedDriver!,
            (int)reader["Id"]
            
        );
    }

   /// <summary>
   /// Retrieves the list of tariffs available for the specified automobile from the database.
   /// </summary>
   /// <param name="automobileId">The unique identifier of the automobile for which to load available tariffs.</param>
   /// <param name="connection">An open SQL connection used to execute the query. The connection must remain open for the duration of the method
   /// call.</param>
   /// <returns>A list of Tariff objects representing the tariffs available for the specified automobile. The list is empty if no
   /// tariffs are available.</returns>
    static private List<Tariff> LoadAvailableTariffs(int automobileId, SqlConnection connection)
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

    static private void UpdateAvailableTariffs(Automobile entity, SqlConnection connection)
    {
        const string sqlDelete = "DELETE FROM TariffAvailability WHERE automobileId = @aId";
        using var cmdDel = new SqlCommand(sqlDelete, connection);
        cmdDel.Parameters.AddWithValue("@aId", entity.Id);
        cmdDel.ExecuteNonQuery();

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
}