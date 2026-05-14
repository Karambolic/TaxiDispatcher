using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BusinessLogic.Services;
using Domain.Entities;
using UI.Infrastructure;

namespace UI.ViewModels;

public class CreateOrderViewModel : ViewModelBase
{
    private readonly ClientService _clientService;
    private readonly OrderService _orderService;
    private readonly TariffService _tariffService;
    private readonly AddressService _addressService;
    private readonly DispatcherService _dispatcherService;

    // Form fields 
    private string _firstName = "";
    private string _lastName = "";
    private string _phone = "";

    private string _startStreet = "";
    private string _startNumber = "";
    private string _endStreet = "";
    private string _endNumber = "";

    private int _passengersCount = 1; // Default passenger count is 1
    private string _commentary = "";
    private Tariff? _selectedTariff;

    // Properties bound to the UI
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }
    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

    // Start address textboxes
    public string StartStreet { get => _startStreet; set => SetProperty(ref _startStreet, value); }
    public string StartNumber { get => _startNumber; set => SetProperty(ref _startNumber, value); }

    // End address textboxes
    public string EndStreet { get => _endStreet; set => SetProperty(ref _endStreet, value); }
    public string EndNumber { get => _endNumber; set => SetProperty(ref _endNumber, value); }

    public int PassengersCount { get => _passengersCount; set => SetProperty(ref _passengersCount, value); }
    public string Commentary { get => _commentary; set => SetProperty(ref _commentary, value); }

    public ObservableCollection<Tariff> AvailableTariffs { get; } = new();

    public Tariff? SelectedTariff
    {
        get => _selectedTariff;
        set => SetProperty(ref _selectedTariff, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestClose;

    public CreateOrderViewModel(
        ClientService clientService,
        OrderService orderService,
        TariffService tariffService,
        AddressService addressService,
        DispatcherService dispatcherService)
    {
        _clientService = clientService;
        _orderService = orderService;
        _tariffService = tariffService;
        _addressService = addressService;
        _dispatcherService = dispatcherService;

        SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());

        LoadTariffs();
    }

    private void LoadTariffs()
    {
        var tariffs = _tariffService.GetAllTariffs();
        foreach (var t in tariffs)
            AvailableTariffs.Add(t);
    }

    private bool CanSave()
    {
        // Require phone, street names, house numbers, and a tariff
        return !string.IsNullOrWhiteSpace(Phone) &&
               !string.IsNullOrWhiteSpace(StartStreet) &&
               !string.IsNullOrWhiteSpace(StartNumber) &&
               !string.IsNullOrWhiteSpace(EndStreet) &&
               !string.IsNullOrWhiteSpace(EndNumber) &&
               SelectedTariff != null &&
               PassengersCount > 0;
    }

    private void ExecuteSave()
    {
        try
        {
            // Resolve the client
            var client = _clientService.GetOrCreateClient(FirstName, LastName, Phone);

            // Resolve addresses via AddressService (prevents DB duplicates)
            var pickup = _addressService.GetOrCreateAddress(StartStreet, StartNumber);
            var destination = _addressService.GetOrCreateAddress(EndStreet, EndNumber);

            // Get dispatchersession data
            var currentDispatcher = _dispatcherService.CurrentDispatcher;
            if (currentDispatcher == null)
            {
                MessageBox.Show("Error: No active dispatcher session found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create the order
            var newOrder = _orderService.CreateOrder(
                currentDispatcher.Id,
                client.Id,
                pickup.Id,
                destination.Id,
                SelectedTariff!.Id,
                PassengersCount,
                Commentary);

            MessageBox.Show($"Order #{newOrder.Id} created successfully!", "Success");
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}