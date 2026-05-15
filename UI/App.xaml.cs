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
        // Catch exceptions thrown on background threads
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            MessageBox.Show(ex.ExceptionObject?.ToString(), "Unhandled Exception");

        // Catch exceptions thrown on the UI thread via Dispatcher
        DispatcherUnhandledException += (s, ex) =>
        {
            MessageBox.Show(ex.Exception?.ToString(), "UI Exception");
            ex.Handled = true; // Prevents app from closing
        };

        base.OnStartup(e);

        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        string connectionString = Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddSingleton(new DbConnectionFactory(connectionString));

        // Session is singleton — it holds who is logged in
        services.AddSingleton<DispatcherSession>();

        // UnitOfWork and all services are transient
        services.AddTransient<UnitOfWork>();

        services.AddTransient<DispatcherService>();
        services.AddTransient<AddressService>();
        services.AddTransient<ClientService>();
        services.AddTransient<DriverService>();
        services.AddTransient<OrderService>();
        services.AddTransient<TariffService>();
        services.AddTransient<TransactionService>();
        services.AddTransient<ReportService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<CreateOrderViewModel>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<CreateOrderWindow>();
    }
}
