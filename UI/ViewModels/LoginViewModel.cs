using Application.Services;
using System.Windows;
using System.Windows.Input;
using UI.ViewModels;

namespace TaxiDispatcher.WPF.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly DispatcherService _dispatcherService;
    private string _phoneNumber = "";
    private string _password = "";

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    // Команда для кнопки "Увійти"
    public ICommand LoginCommand { get; }

    public LoginViewModel(DispatcherService dispatcherService)
    {
        _dispatcherService = dispatcherService;
        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
    }

    private bool CanExecuteLogin(object? obj) =>
        !string.IsNullOrWhiteSpace(PhoneNumber) && !string.IsNullOrWhiteSpace(Password);

    private void ExecuteLogin(object? obj)
    {
        if (_dispatcherService.Login(PhoneNumber, Password))
        {
            // Логіка переходу на головне вікно
            MessageBox.Show($"Вітаємо, {_dispatcherService.CurrentDispatcher.FirstName}!");
        }
        else
        {
            MessageBox.Show("Невірний номер або пароль");
        }
    }
}