using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class DriverService(UnitOfWork unitOfWork)
{
    /// <summary>
    /// Register a new driver in the system. Initially, the driver has no assigned vehicle and is marked as free.
    /// </summary>
    public Driver AddNewDriver(string firstName, string lastName, string phoneNumber)
    {
        var driver = new Driver(firstName, lastName, phoneNumber, 0, DriverStatus.Free);
        unitOfWork.Drivers.Add(driver);
        return driver;
    }

    /// <summary>
    /// Register a vehicle for a driver. This method associates an automobile with a driver and specifies which tariffs the vehicle can operate under.
    /// </summary>
    public bool RegisterVehicleForDriver(int driverId, Automobile auto, List<Tariff> allowedTariffs)
    {
        var driver = unitOfWork.Drivers.GetById(driverId);
        if (driver == null) 
            return false;

        // Add tariffs to the automobile
        auto.AllowedTariffs = allowedTariffs;

        // Update the driver, linking the new automobile with the driver
        driver.Status = DriverStatus.Free;
        unitOfWork.Drivers.Update(driver);

        // Update the automobile, linking it to the driver and save it to the database
        auto.Driver = driver;
        unitOfWork.Automobiles.Add(auto);

        return true;
    }

    /// <summary>
    /// Get a list of drivers who are currently free and have vehicles that can operate under a specific tariff
    /// </summary>
    public List<Driver> GetFreeDriversByTariff(int tariffId)
    {
        var freeDrivers = unitOfWork.Drivers.GetByStatus(DriverStatus.Free);

        return freeDrivers.Where(d => {
            // Look for the car where DriverId == d.Id
            var auto = unitOfWork.Automobiles.GetAll()
                        .FirstOrDefault(a => a.Driver?.Id == d.Id);

            return auto?.AllowedTariffs.Any(t => t.Id == tariffId) ?? false;
        }).ToList();
    }

    public List<Driver> GetAllFreeDrivers() => unitOfWork.Drivers.GetByStatus(DriverStatus.Free);

    public bool ChangeDriverStatus(int driverId, DriverStatus newStatus)
    {
        var driver = unitOfWork.Drivers.GetById(driverId);
        if (driver == null) 
            return false;

        driver.Status = newStatus;
        return unitOfWork.Drivers.Update(driver);
    }

    /// <summary>
    /// Full termination of a driver's contract
    /// </summary>
    public bool TerminateContract(int driverId)
    {
        var driver = unitOfWork.Drivers.GetById(driverId);
        if (driver == null) return false;

        var auto = unitOfWork.Automobiles.GetAll().FirstOrDefault(a => a.Driver?.Id == driverId);

        // Delete the automobile associated with the driver, if it exists
        if (auto != null)
            unitOfWork.Automobiles.Delete(auto.Id);

        // Delete the driver
        return unitOfWork.Drivers.Delete(driverId);
    }
}
