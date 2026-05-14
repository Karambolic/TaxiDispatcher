using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class TariffService(UnitOfWork unitOfWork)
{
    /// <summary>
    /// Get all available tariffs in the system
    /// </summary>
    public List<Tariff> GetAllTariffs()
    {
        return unitOfWork.Tariffs.GetAll();
    }

    /// <summary>
    /// Get a specific tariff by its ID. Returns null if not found
    /// </summary>
    public Tariff? GetTariffById(int id)
    {
        return unitOfWork.Tariffs.GetById(id);
    }

    /// <summary>
    /// Update the price of a specific tariff. Returns true if successful, false otherwise
    /// </summary>
    public bool UpdateTariffPrice(int tariffId, decimal newPrice)
    {
        if (newPrice < 0)
            return false;

        var tariff = unitOfWork.Tariffs.GetById(tariffId);
        if (tariff == null)
            return false;

        tariff.PricePerKm = newPrice;
        return unitOfWork.Tariffs.Update(tariff);
    }

    /// <summary>
    /// Add a new tariff to the system with the specified name and price per kilometer
    /// </summary>
    public void CreateTariff(string name, decimal pricePerKm)
    {
        var tariff = new Tariff(name, pricePerKm);
        unitOfWork.Tariffs.Add(tariff);
    }

    /// <summary>
    /// Get all tariffs that are allowed for a specific automobile. 
    /// Returns an empty list if the automobile is not found or has no allowed tariffs
    /// </summary>
    public List<Tariff> GetTariffsByAutomobile(int automobileId)
    {
        var auto = unitOfWork.Automobiles.GetById(automobileId);
        return auto?.AllowedTariffs ?? new List<Tariff>();
    }
}