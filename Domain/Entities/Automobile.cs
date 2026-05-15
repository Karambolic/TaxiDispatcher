namespace Domain.Entities;

public class Automobile
{
    public int Id { get; set; }
    public string PlateNumber { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int Capacity { get; set; }
    public Driver? Driver { get; set; }

    // Tariffs the automobile is allowed to perform orders for
    public List<Tariff> AllowedTariffs { get; set; } = new();

    public Automobile(string plateNumber, string make, string model, int year, int capacity, Driver driver, int id = 0)
    {
        PlateNumber = plateNumber;
        Make = make;
        Model = model;
        Year = year;
        Capacity = capacity;
        Driver = driver;
        Id = id;
    }

    public Automobile() { }
}