namespace Domain.DTO;

// For query 6.1
public class AutoDriverReport
{
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public string DriverLastName { get; set; } = "";
}

// For Query 6.3
public class OrderPeriodReport
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal FinalPrice { get; set; }
}
// For query 6.5
public class TariffAvgPriceReport
{
    public string Name { get; set; } = "";
    public decimal AvgPrice { get; set; }
}

// For query 6.7
public class ClientMaxOrderReport
{
    public int ClientId { get; set; }
    public int OrderId { get; set; }
    public decimal FinalPrice { get; set; }
}

// For query 6.8
public class IdleDriverReport
{
    public int Id { get; set; }
    public string LastName { get; set; } = "";
}

// For query 6.9
public class TariffStatusReport
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
}