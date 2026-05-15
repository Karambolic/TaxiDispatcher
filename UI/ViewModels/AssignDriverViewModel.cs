using BusinessLogic.Services;
using Domain.Entities;
using System.Collections.ObjectModel;

namespace UI.ViewModels;

public class AssignDriverViewModel : ViewModelBase
{
    private readonly DriverService _driverService;
    private Driver? _selectedDriver;

    public ObservableCollection<Driver> Drivers { get; } = new();

    public Driver? SelectedDriver
    {
        get => _selectedDriver;
        set
        {
            _selectedDriver = value;
            OnPropertyChanged();
        }
    }

    public AssignDriverViewModel(DriverService driverService)
    {
        _driverService = driverService;
        LoadDrivers();
    }

    private void LoadDrivers()
    {
        var availableDrivers = _driverService.GetAllFreeDrivers();

        Drivers.Clear();
        foreach (var driver in availableDrivers)
        {
            Drivers.Add(driver);
        }
    }
}