using Domain.DTO;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    List<Order> GetActiveOrders();
    List<Order> GetOrdersByClientId(int clientId);
    List<OrderPeriodReport> GetOrdersByPeriod(DateTime start, DateTime end);
}