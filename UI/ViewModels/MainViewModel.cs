using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BusinessLogic.Services;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using UI.Infrastructure;
using UI.Views;

namespace UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly OrderService _orderService;
    private readonly DispatcherService _dispatcherService;
    private readonly ClientService _clientService;
    private readonly ReportService _reportService;
    private readonly DriverService _driverService;

    private Order? _selectedOrder;
    private object? _reportData;

    private DateTime _startDate = DateTime.Now.AddDays(-7); // Default range is last week
    private DateTime _endDate = DateTime.Now;

    public ObservableCollection<Order> Orders { get; } = new();

    public Order? SelectedOrder
    {
        get => _selectedOrder;
        set => SetProperty(ref _selectedOrder, value);
    }

    public string DispatcherName => _dispatcherService.CurrentDispatcher?.FirstName ?? "Dispatcher";

    public object? ReportData
    {
        get => _reportData;
        set => SetProperty(ref _reportData, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    // Commands to be binded to UI buttons
    public ICommand RefreshOrdersCommand { get; }
    public ICommand CreateOrderCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand AssignDriverCommand { get; }
    public ICommand LogoutCommand { get; }

    public Dictionary<string, ICommand> ReportCommands { get; } = new();

    public MainViewModel(
        OrderService orderService,
        DispatcherService dispatcherService,
        ClientService clientService,
        ReportService reportService,
        DriverService driverService)
    {
        _orderService = orderService;
        _dispatcherService = dispatcherService;
        _clientService = clientService;
        _reportService = reportService;
        _driverService = driverService;

        RefreshOrdersCommand = new RelayCommand(_ =>
        {
            try
            {
                LoadActiveOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load orders:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        CreateOrderCommand = new RelayCommand(_ => ExecuteOpenCreateOrder());

        CancelOrderCommand = new RelayCommand(_ => ExecuteCancel(), _ => SelectedOrder != null);

        // Assign driver button is active ONLY if an order is selected AND its status is New
        AssignDriverCommand = new RelayCommand(
            _ => ExecuteAssignDriver(),
            _ => SelectedOrder != null && SelectedOrder.Status == OrderStatus.New
        );

        LogoutCommand = new RelayCommand(_ => ExecuteLogout());

        InitializeReportCommands();
        LoadActiveOrders();
    }

    private void LoadActiveOrders()
    {
        var data = _orderService.GetActiveOrders();
        Orders.Clear();
        foreach (var order in data)
            Orders.Add(order);
    }

    private void ExecuteAssignDriver()
    {
        // Defensive check to protect against accidental execution
        if (SelectedOrder == null || SelectedOrder.Status != OrderStatus.New)
            return;

        // Get the Window and its VM from the DI container
        var viewModel = App.ServiceProvider.GetRequiredService<AssignDriverViewModel>();
        var dialog = new AssignDriverWindow(viewModel);
        dialog.Owner = Application.Current.MainWindow;

        if (dialog.ShowDialog() == true)
        {
            var driver = viewModel.SelectedDriver;
            if (driver != null)
            {
                _orderService.AssignDriverToOrder(SelectedOrder.Id, driver.Id);
                MessageBox.Show($"Driver {driver.FirstName} assigned to order #{SelectedOrder.Id}");
                LoadActiveOrders();
            }
        }
    }

    private void ExecuteOpenCreateOrder()
    {
        var dialog = App.ServiceProvider.GetRequiredService<CreateOrderWindow>();
        dialog.ShowDialog(); // Open as modal dialog
        LoadActiveOrders(); // Refresh orders after creating (or not, anyway) a new one
    }

    private void ExecuteCancel()
    {
        if (SelectedOrder == null)
            return;

        _orderService.CancelOrder(SelectedOrder.Id);
        MessageBox.Show($"Order #{SelectedOrder.Id} canceled.");
        LoadActiveOrders();
    }

    private void ExecuteLogout()
    {
        var result = MessageBox.Show("Sure you want to log out?", "Logout",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _dispatcherService.Logout();

            // Open login window
            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            // Close current mainWindow
            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = loginWindow;
        }
    }

    private void InitializeReportCommands()
    {
        ReportCommands.Add("1", new RelayCommand(_ => ReportData = _reportService.GetFleetAnalytics()));
        ReportCommands.Add("2", new RelayCommand(_ => ReportData = _reportService.GetMarketingClients("+38050")));
        ReportCommands.Add("3", new RelayCommand(_ => ReportData = _reportService.GetPeriodReport(StartDate, EndDate)));
        ReportCommands.Add("4", new RelayCommand(_ => MessageBox.Show($"Total system clients: {_clientService.GetTotalClientsCount()}")));
        ReportCommands.Add("5", new RelayCommand(_ => ReportData = _reportService.GetTariffPerformance()));
        ReportCommands.Add("6", new RelayCommand(_ => {
            var leader = _reportService.GetTopDispatcher();
            ReportData = leader != null ? new List<object> { leader } : null;
        }));
        ReportCommands.Add("7", new RelayCommand(_ => ReportData = _reportService.GetHighValueOrders()));
        ReportCommands.Add("8", new RelayCommand(_ => ReportData = _reportService.GetDriversOnStandby()));
        ReportCommands.Add("9", new RelayCommand(_ => ReportData = _reportService.GetTariffUsage()));
    }
}