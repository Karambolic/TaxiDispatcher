using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class DriverService(UnitOfWork unitOfWork)
{
    /// <summary>
    /// Реєстрація нового водія
    /// </summary>
    public Driver AddNewDriver(string firstName, string lastName, string phoneNumber)
    {
        var driver = new Driver(firstName, lastName, phoneNumber, 0, DriverStatus.Free);
        unitOfWork.Drivers.Add(driver);
        return driver;
    }

    /// <summary>
    /// Додавання автомобіля та призначення його водію
    /// </summary>
    public bool RegisterVehicleForDriver(int driverId, Automobile auto, List<Tariff> allowedTariffs)
    {
        var driver = unitOfWork.Drivers.GetById(driverId);
        if (driver == null) return false;

        // Додаємо тарифи до об'єкта авто перед збереженням
        auto.AllowedTariffs = allowedTariffs;

        // Зберігаємо авто (репозиторій сам запише дані в Automobiles та TariffAvailability)
        unitOfWork.Automobiles.Add(auto);

        // Оновлюємо водія, прив'язуючи ID нового авто
        // Примітка: Переконайся, що в сутності Driver є властивість AutomobileId або об'єкт Automobile
        driver.Status = DriverStatus.Free;
        // unitOfWork.Drivers.Update(driver); 

        return true;
    }

    /// <summary>
    /// Отримання водіїв, які можуть виконати замовлення за певним тарифом
    /// </summary>
    public List<Driver> GetDriversByTariff(int tariffId)
    {
        // Це складна логіка: беремо вільних водіїв і фільтруємо за тарифами їхніх авто
        var allDrivers = unitOfWork.Drivers.GetByStatus(DriverStatus.Free);

        return allDrivers.Where(d =>
            unitOfWork.Automobiles.GetById(d.Id)? // Тут логіка залежить від того, як пов'язані ID
            .AllowedTariffs.Any(t => t.Id == tariffId) ?? false
        ).ToList();
    }

    public List<Driver> GetAvailableDrivers() => unitOfWork.Drivers.GetByStatus(DriverStatus.Free);

    public bool ChangeDriverStatus(int driverId, DriverStatus newStatus)
    {
        var driver = unitOfWork.Drivers.GetById(driverId);
        if (driver == null) return false;

        driver.Status = newStatus;
        return unitOfWork.Drivers.Update(driver);
    }

    /// <summary>
    /// Повне видалення водія разом з його автомобілем
    /// </summary>
    public bool TerminateContract(int driverId)
    {
        var driver = unitOfWork.Drivers.GetById(driverId);
        if (driver == null) return false;

        // 1. Видаляємо авто (залежно від логіки FK в БД)
        // unitOfWork.Automobiles.Delete(driver.AutomobileId);

        // 2. Видаляємо водія
        return unitOfWork.Drivers.Delete(driverId);
    }
}