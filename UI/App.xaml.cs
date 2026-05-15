using BusinessLogic.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;

    public App()
    {
        // Build the configuration object
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database connection logic - read the string from appsettings.json
        string connectionString = Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Inject the connection string into db connection factory
        services.AddSingleton(new DbConnectionFactory(connectionString));

        // DI for unit of ork
        services.AddScoped<UnitOfWork>();

        // DI for repository interfaces and their implementations
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IDispatcherRepository, DispatcherRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IRepository<Address>, AddressRepository>();
        services.AddScoped<IRepository<Tariff>, TariffRepository>();
        services.AddScoped<IRepository<Transaction>, TransactionRepository>();

        services.AddScoped<AutomobileRepository>();
        services.AddScoped<ReportRepository>();

        // Business logic (Services). DispatcherService is Singleton so it keeps the logged-in user state across the app
        services.AddSingleton<DispatcherService>();

        services.AddTransient<AddressService>();
        services.AddTransient<ClientService>();
        services.AddTransient<DriverService>();
        services.AddTransient<OrderService>();
        services.AddTransient<TariffService>();
        services.AddTransient<TransactionService>();
        services.AddTransient<ReportService>();

        // UI
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<CreateOrderViewModel>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<CreateOrderWindow>();
    }
}
