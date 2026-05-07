using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure;

public class UnitOfWork(DbConnectionFactory factory)
{
    // private fields for lazy loading 
    private IOrderRepository? _orders;
    private IClientRepository? _clients;
    private IDriverRepository? _drivers;
    private IRepository<Address>? _addresses;
    private IRepository<Tariff>? _tariffs;
    private IRepository<Transaction>? _transactions;
    private IRepository<Automobile>? _automobiles;
    private IDispatcherRepository? _dispatchers;

    // Properties to access the repositories, initializing in the lazy way (upon real need - when called)
    public IOrderRepository Orders => _orders ??= new OrderRepository(factory);

    public IClientRepository Clients => _clients ??= new ClientRepository(factory);

    public IDriverRepository Drivers => _drivers ??= new DriverRepository(factory);

    public IRepository<Address> Addresses => _addresses ??= new AddressRepository(factory);

    public IRepository<Tariff> Tariffs => _tariffs ??= new TariffRepository(factory);

    public IRepository<Transaction> Transactions => _transactions ??= new TransactionRepository(factory);

    public IRepository<Automobile> Automobiles => _automobiles ??= new AutomobileRepository(factory);

    public IDispatcherRepository Dispatchers => _dispatchers ??= new DispatcherRepository(factory);
}