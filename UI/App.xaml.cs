using BusinessLogic.Services;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using UI.ViewModels; // Переконайся, що шлях до ViewModels правильний
using UI.Views;        // Переконайся, що шлях до Windows правильний

namespace UI;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Дістаємо LoginWindow з контейнера. 
        // DI автоматично підставить у нього LoginViewModel, а в неї — DispatcherService.
        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. Інфраструктура
        string connectionString = "Data Source=.;Initial Catalog=TaxiDB;Integrated Security=True;TrustServerCertificate=True";
        services.AddSingleton(new DbConnectionFactory(connectionString));
        services.AddScoped<UnitOfWork>();

        // 2. Бізнес-логіка (Сервіси)
        services.AddTransient<OrderService>();
        services.AddTransient<DriverService>();
        services.AddTransient<ClientService>();
        services.AddTransient<TariffService>();
        services.AddTransient<AddressService>();
        services.AddTransient<DispatcherService>();
        services.AddTransient<TransactionService>();

        // 3. ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();

        // 4. Windows (Важливо: вікна теж мають бути в DI)
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginWindow>();
    }
}