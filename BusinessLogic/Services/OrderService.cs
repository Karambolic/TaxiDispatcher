using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class OrderService(UnitOfWork uow)
{
    public Order CreateOrder(int dispatcherId, int clientId, int startAddrId, int endAddrId, int tariffId, int passengers, string commentary)
    {
        var dispatcher = uow.Dispatchers.GetById(dispatcherId);
        var client = uow.Clients.GetById(clientId);
        var start = uow.Addresses.GetById(startAddrId);
        var end = uow.Addresses.GetById(endAddrId);
        var tariff = uow.Tariffs.GetById(tariffId);

        float distance = EstimateDistance(start, end);
        decimal price = CalculatePrice(distance, tariff);

        var order = new Order
        {
            Dispatcher = dispatcher!,
            Client = client!,
            AddressStart = start!,
            AddressEnd = end!,
            Tariff = tariff!,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow,
            PassengerCount = passengers,
            Comment = commentary,
            FinalDistanceKm = distance,
            FinalPrice = price
        };

        uow.Orders.Add(order);
        return order;
    }

    /// <summary>
    /// Update order details such as addresses, tariff, or passenger count. Recalculates price and distance if necessary
    /// </summary>
    public bool UpdateOrderDetails(Order order)
    {
        if (order == null || order.Id <= 0) return false;

        // recalculate distance and price if addresses or tariff have changed
        order.FinalDistanceKm = EstimateDistance(order.AddressStart, order.AddressEnd);
        order.FinalPrice = CalculatePrice(order.FinalDistanceKm, order.Tariff);

        return uow.Orders.Update(order);
    }

    public bool AssignDriverToOrder(int orderId, int driverId)
    {
        var order = uow.Orders.GetById(orderId);
        var driver = uow.Drivers.GetById(driverId);

        if (order == null || driver == null || driver.Status != DriverStatus.Free)
            return false;

        order.Driver = driver;
        order.Status = OrderStatus.InWork;
        order.StartedAt = DateTime.UtcNow;

        uow.Orders.Update(order);

        driver.Status = DriverStatus.Busy;
        uow.Drivers.Update(driver);

        return true;
    }

    public bool FinishOrder(int orderId)
    {
        var order = uow.Orders.GetById(orderId);
        if (order == null || order.Status != OrderStatus.InWork) 
            return false;

        order.Status = OrderStatus.Finished;
        order.FinishedAt = DateTime.UtcNow;

        if (order.Driver != null)
        {
            order.Driver.Status = DriverStatus.Free;
            uow.Drivers.Update(order.Driver);
        }

        var transaction = new Transaction
        {
            TransactionType = TransactionType.OrderPaymentByClient,
            DriverId = order.Driver?.Id ?? 0,
            ClientId = order.Client.Id,
            Amount = order.FinalPrice,
            Comment = $"Payment for order #{order.Id}",
            Timestamp = DateTime.UtcNow
        };
        
        uow.Transactions.Add(transaction);

        return uow.Orders.Update(order);
    }

    /// <summary>
    /// Get all active orders (status = New or InWork)
    /// </summary>
    /// <returns></returns>
    public List<Order> GetActiveOrders()
    {
        return uow.Orders.GetAll()
            .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.InWork)
            .ToList();
    }

    /// <summary>
    /// Get all orders that are awaiting assignment (have status = New)
    /// </summary>
    /// <returns></returns>
    public List<Order> GetAwaitingOrder()
    {
        return uow.Orders.GetAll()
            .Where(o => o.Status == OrderStatus.New).ToList();
    }


    // Aux methods for distance and price calculation

    private float EstimateDistance(Address? start, Address? end)
    {
        if (start == null || end == null) return 0f;

        // TODO: add a real distance calculation based on coordinates or an API in future
        float distance = (float)Random.Shared.Next(3,20); // For now it's random distance between 3 and 20 km
        return distance;
    }

    private decimal CalculatePrice(float distance, Tariff? tariff)
    {
        if (tariff == null) return 0m;

        // Base lofic: distance * prace per km got from the tariff
        decimal totalPrice = (decimal)distance * tariff.PricePerKm;

        // TODO: Add real external value for minimal order price.
        // For now it's like 50 uah
        decimal minimalPrice = 50.0m; 

        return totalPrice > 0 ? totalPrice : minimalPrice;
    }
}