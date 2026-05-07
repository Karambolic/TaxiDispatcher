using Domain.Entities;

namespace Domain.Interfaces;

public interface IDriverRepository : IRepository<Driver>
{
    List<Driver> GetByStatus(DriverStatus status);
}