using BusinessLogic.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
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
        // 1. Build the configuration object
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        // 2. Setup DI
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
        // --- 1. Connection Logic ---
        // Read the string from appsettings.json instead of hardcoding
        string connectionString = Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Inject the connection string into your factory
        services.AddSingleton(new DbConnectionFactory(connectionString));

        // --- 2. Infrastructure ---
        services.AddScoped<UnitOfWork>();

        // --- 3. Repositories ---
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IDispatcherRepository, DispatcherRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IRepository<Address>, AddressRepository>();
        services.AddScoped<IRepository<Tariff>, TariffRepository>();
        services.AddScoped<IRepository<Transaction>, TransactionRepository>();

        services.AddScoped<AutomobileRepository>();
        services.AddScoped<ReportRepository>();

        // --- 4. Business Logic (Services) ---
        // Changed DispatcherService to Singleton so it keeps the logged-in user state across the app
        services.AddSingleton<DispatcherService>();

        services.AddTransient<AddressService>();
        services.AddTransient<ClientService>();
        services.AddTransient<DriverService>();
        services.AddTransient<OrderService>();
        services.AddTransient<TariffService>();
        services.AddTransient<TransactionService>();
        services.AddTransient<ReportService>();

        // --- 5. Presentation & UI ---
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<CreateOrderViewModel>(); // Don't forget this one!

        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<CreateOrderWindow>();
    }
}